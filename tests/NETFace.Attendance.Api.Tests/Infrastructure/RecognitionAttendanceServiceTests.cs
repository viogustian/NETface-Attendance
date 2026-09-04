using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NETFace.Attendance.Application.DTOs;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Infrastructure.Persistence;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class RecognitionAttendanceServiceTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private class StubDetectionService(bool detected = true) : IFaceDetectionService
    {
        public Task<FaceDetectionResult> DetectFacesAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FaceDetectionResult(detected, detected ? 1 : 0));
        }
    }

    private class StubEmbeddingExtractor(float[]? embeddingToReturn = null, Exception? exceptionToThrow = null) : IFaceEmbeddingExtractor
    {
        public Task<float[]> ExtractEmbeddingAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            if (exceptionToThrow != null) throw exceptionToThrow;
            return Task.FromResult(embeddingToReturn ?? [0.5f, 0.5f]);
        }
    }

    private class StubMatchingService(bool isMatch = false, Guid? matchedId = null, double distance = 0.2) : IFaceMatchingService
    {
        public FaceDetectionResult DetectFace(byte[] imageBytes) => new(true, 1);

        public double CalculateDistance(float[] vectorA, float[] vectorB) => distance;

        public FaceMatchResult Match(float[] vectorA, float[] vectorB, double? threshold = null) =>
            new(isMatch, distance);

        public FaceMatchResult FindBestMatch(float[] targetVector, IEnumerable<Employee> candidates, double? threshold = null) =>
            new(isMatch, distance, isMatch ? matchedId : null);
    }

    [Fact]
    public async Task AttemptRecognitionAsync_WhenMatchedEmployeeIsInactive_ReturnsAccessDeniedInactiveEmployee()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var employee = new Employee("EMP-INACTIVE", "Inactive Person", false);
        employee.AddFaceEmbedding([0.5f, 0.5f]);
        employee.Deactivate();
        db.Employees.Add(employee);

        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), [(employee.Id, employee.EmployeeCode, employee.FullName)]);
        db.AttendanceSessions.Add(session);
        await db.SaveChangesAsync();

        var sut = new RecognitionAttendanceService(
            db,
            new StubDetectionService(true),
            new StubEmbeddingExtractor([0.5f, 0.5f]),
            new StubMatchingService(isMatch: true, matchedId: employee.Id, distance: 0.1),
            new SpoofingDetectionService(),
            NullLogger<RecognitionAttendanceService>.Instance);

        // Act
        var result = await sut.AttemptRecognitionAsync([1, 2, 3], session.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Access Denied — Inactive Employee", result.Message);
        Assert.Equal(employee.Id, result.EmployeeId);

        var log = await db.RecognitionLogs.SingleAsync();
        Assert.False(log.IsSuccess);
        Assert.Equal("Access Denied — Inactive Employee", log.ErrorMessage);
    }

    [Fact]
    public async Task AttemptRecognitionAsync_WhenConsecutiveUnknownFacesReachThreshold_ReturnsConsecutiveFailureAndLogsAudit()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), []);
        db.AttendanceSessions.Add(session);
        await db.SaveChangesAsync();

        var spoofingService = new SpoofingDetectionService(threshold: 3);
        var sut = new RecognitionAttendanceService(
            db,
            new StubDetectionService(true),
            new StubEmbeddingExtractor([0.5f, 0.5f]),
            new StubMatchingService(isMatch: false), // Unknown face
            spoofingService,
            NullLogger<RecognitionAttendanceService>.Instance);

        // Act 1 & 2: Before threshold
        var res1 = await sut.AttemptRecognitionAsync([1, 2, 3], session.Id);
        var res2 = await sut.AttemptRecognitionAsync([1, 2, 3], session.Id);

        Assert.False(res1.Success);
        Assert.False(res1.IsConsecutiveFailure);
        Assert.False(res2.Success);
        Assert.False(res2.IsConsecutiveFailure);

        // Act 3: Threshold reached (consecutive unknown face)
        var res3 = await sut.AttemptRecognitionAsync([1, 2, 3], session.Id);

        // Assert
        Assert.False(res3.Success);
        Assert.True(res3.IsConsecutiveFailure);
        Assert.Contains("Consecutive unknown faces", res3.Message);

        var logs = await db.RecognitionLogs.ToListAsync();
        Assert.Equal(3, logs.Count);
        Assert.Contains("spoofing", logs.Last().ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AttemptRecognitionAsync_WhenModelFileMissingOrCorrupted_ReturnsFallbackToPinMode()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), []);
        db.AttendanceSessions.Add(session);
        await db.SaveChangesAsync();

        var sut = new RecognitionAttendanceService(
            db,
            new StubDetectionService(true),
            new StubEmbeddingExtractor(exceptionToThrow: new FileNotFoundException("Model file missing")),
            new StubMatchingService(),
            new SpoofingDetectionService(),
            NullLogger<RecognitionAttendanceService>.Instance);

        // Act
        var result = await sut.AttemptRecognitionAsync([1, 2, 3], session.Id);

        // Assert
        Assert.False(result.Success);
        Assert.True(result.FallbackToPin);
        Assert.Contains("PIN", result.Message, StringComparison.OrdinalIgnoreCase);

        var log = await db.RecognitionLogs.SingleAsync();
        Assert.False(log.IsSuccess);
        Assert.Contains("PIN", log.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AttemptRecognitionAsync_WhenMemoryExhaustionOOM_ReturnsFallbackToPinMode()
    {
        // Arrange
        using var db = CreateInMemoryDb();
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), []);
        db.AttendanceSessions.Add(session);
        await db.SaveChangesAsync();

        var sut = new RecognitionAttendanceService(
            db,
            new StubDetectionService(true),
            new StubEmbeddingExtractor(exceptionToThrow: new OutOfMemoryException("Native memory exhausted")),
            new StubMatchingService(),
            new SpoofingDetectionService(),
            NullLogger<RecognitionAttendanceService>.Instance);

        // Act
        var result = await sut.AttemptRecognitionAsync([1, 2, 3], session.Id);

        // Assert
        Assert.False(result.Success);
        Assert.True(result.FallbackToPin);
        Assert.Contains("PIN", result.Message, StringComparison.OrdinalIgnoreCase);
    }
}
