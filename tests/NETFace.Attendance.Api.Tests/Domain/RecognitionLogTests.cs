using System;
using NETFace.Attendance.Domain.Entities;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Domain;

public class RecognitionLogTests
{
    // --- Seam: RecognitionLog public interface ---

    [Fact]
    public void CreateSuccess_ShouldPopulatePropertiesCorrectly()
    {
        var employeeId = Guid.NewGuid();
        var employeeCode = "EMP001";
        var confidence = 0.95;
        var processingTimeMs = 45L;
        var attemptedAt = DateTimeOffset.UtcNow;

        var log = RecognitionLog.CreateSuccess(employeeId, employeeCode, confidence, processingTimeMs, attemptedAt);

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal(employeeId, log.EmployeeId);
        Assert.Equal(employeeCode, log.MatchedEmployeeCode);
        Assert.True(log.IsSuccess);
        Assert.Equal(confidence, log.Confidence);
        Assert.Equal(processingTimeMs, log.ProcessingTimeMs);
        Assert.Equal(attemptedAt, log.AttemptedAt);
        Assert.Null(log.ErrorMessage);
    }

    [Fact]
    public void CreateFailed_ShouldPopulatePropertiesCorrectly()
    {
        var errorMessage = "No face detected";
        var processingTimeMs = 12L;
        var attemptedAt = DateTimeOffset.UtcNow;

        var log = RecognitionLog.CreateFailed(errorMessage, processingTimeMs, attemptedAt);

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Null(log.EmployeeId);
        Assert.Null(log.MatchedEmployeeCode);
        Assert.False(log.IsSuccess);
        Assert.Equal(0, log.Confidence);
        Assert.Equal(processingTimeMs, log.ProcessingTimeMs);
        Assert.Equal(attemptedAt, log.AttemptedAt);
        Assert.Equal(errorMessage, log.ErrorMessage);
    }

    [Fact]
    public void CreateFailed_WithCandidateDetails_ShouldStoreDetails()
    {
        var employeeId = Guid.NewGuid();
        var employeeCode = "EMP002";
        var errorMessage = "Matched employee is inactive";
        var processingTimeMs = 30L;
        var attemptedAt = DateTimeOffset.UtcNow;

        var log = RecognitionLog.CreateFailed(
            errorMessage,
            processingTimeMs,
            attemptedAt,
            employeeId: employeeId,
            matchedEmployeeCode: employeeCode,
            confidence: 0.88);

        Assert.False(log.IsSuccess);
        Assert.Equal(employeeId, log.EmployeeId);
        Assert.Equal(employeeCode, log.MatchedEmployeeCode);
        Assert.Equal(0.88, log.Confidence);
        Assert.Equal(errorMessage, log.ErrorMessage);
    }
}
