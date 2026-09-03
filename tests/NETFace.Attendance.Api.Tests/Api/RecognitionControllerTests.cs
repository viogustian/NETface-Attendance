using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Infrastructure.Persistence;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Api;

public class RecognitionControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RecognitionControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private (HttpClient Client, IServiceProvider Services) CreateClientWithIsolatedDb(
        Action<IServiceCollection>? configureServices = null)
    {
        var dbName = Guid.NewGuid().ToString();
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                configureServices?.Invoke(services);
            });
        });

        return (customFactory.CreateClient(), customFactory.Services);
    }

    [Fact]
    public async Task Attempt_FirstValidRecognition_UpdatesAttendanceEntryToPresent_AndLogsSuccess()
    {
        var (client, services) = CreateClientWithIsolatedDb();
        var imageBytes = new byte[] { 1, 2, 3, 4, 5 };

        var extractor = new DummyFaceEmbeddingExtractor();
        var embedding = await extractor.ExtractEmbeddingAsync(imageBytes);

        Guid employeeId;
        Guid sessionId;

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var employee = new Employee("EMP001", "Alice Wonderland", isAdmin: false);
            employee.AddFaceEmbedding(embedding);
            db.Employees.Add(employee);

            var roster = new List<(Guid, string, string)> { (employee.Id, employee.EmployeeCode, employee.FullName) };
            var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
            db.AttendanceSessions.Add(session);

            await db.SaveChangesAsync();

            employeeId = employee.Id;
            sessionId = session.Id;
        }

        // Post raw image with sessionId query param
        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var response = await client.PostAsync($"/api/recognition/attempt?sessionId={sessionId}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("EMP001", result.EmployeeCode);
        Assert.Equal("Alice Wonderland", result.EmployeeName);
        Assert.NotNull(result.MarkedAt);

        // Verify database persistence
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var updatedSession = await db.AttendanceSessions.Include(s => s.Entries).SingleAsync(s => s.Id == sessionId);
            var entry = Assert.Single(updatedSession.Entries);
            Assert.Equal(AttendanceStatus.Present, entry.Status);
            Assert.Equal(result.MarkedAt, entry.MarkedAt);

            var log = await db.RecognitionLogs.SingleAsync();
            Assert.True(log.IsSuccess);
            Assert.Equal(employeeId, log.EmployeeId);
            Assert.Equal("EMP001", log.MatchedEmployeeCode);
            Assert.Null(log.ErrorMessage);
        }
    }

    [Fact]
    public async Task Attempt_DuplicateRecognition_DoesNotUpdateMarkedAt_AndCreatesAdditionalLog()
    {
        var (client, services) = CreateClientWithIsolatedDb();
        var imageBytes = new byte[] { 10, 20, 30, 40 };

        var extractor = new DummyFaceEmbeddingExtractor();
        var embedding = await extractor.ExtractEmbeddingAsync(imageBytes);

        Guid sessionId;

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var employee = new Employee("EMP001", "Alice Wonderland", isAdmin: false);
            employee.AddFaceEmbedding(embedding);
            db.Employees.Add(employee);

            var roster = new List<(Guid, string, string)> { (employee.Id, employee.EmployeeCode, employee.FullName) };
            var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
            db.AttendanceSessions.Add(session);

            await db.SaveChangesAsync();
            sessionId = session.Id;
        }

        // 1st call (First recognition)
        using (var content1 = new ByteArrayContent(imageBytes))
        {
            content1.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            var firstResponse = await client.PostAsync($"/api/recognition/attempt?sessionId={sessionId}", content1);
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        }

        DateTimeOffset initialMarkedAt;
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.AttendanceSessions.Include(s => s.Entries).SingleAsync(s => s.Id == sessionId);
            initialMarkedAt = session.Entries.Single().MarkedAt!.Value;
        }

        // 2nd call (Duplicate recognition) via Multipart form
        using (var multipart = new MultipartFormDataContent())
        {
            var fileContent = new ByteArrayContent(imageBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            multipart.Add(fileContent, "image", "attempt.jpg");
            multipart.Add(new StringContent(sessionId.ToString()), "sessionId");

            var secondResponse = await client.PostAsync("/api/recognition/attempt", multipart);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

            var result = await secondResponse.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Attendance already marked.", result.Message);
            Assert.Equal(initialMarkedAt, result.MarkedAt);
        }

        // Verify database: MarkedAt has NOT changed, but RecognitionLogs has 2 entries
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.AttendanceSessions.Include(s => s.Entries).SingleAsync(s => s.Id == sessionId);
            Assert.Equal(initialMarkedAt, session.Entries.Single().MarkedAt);

            var logs = await db.RecognitionLogs.OrderBy(l => l.AttemptedAt).ToListAsync();
            Assert.Equal(2, logs.Count);
            Assert.All(logs, l => Assert.True(l.IsSuccess));
        }
    }

    [Fact]
    public async Task Attempt_WhenEmptyImage_ReturnsBadRequest_AndCreatesFailedRecognitionLog()
    {
        var (client, services) = CreateClientWithIsolatedDb();

        using var content = new ByteArrayContent([]);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var response = await client.PostAsync("/api/recognition/attempt", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);

        // Verify failed log was created
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var log = await db.RecognitionLogs.SingleAsync();
            Assert.False(log.IsSuccess);
            Assert.NotNull(log.ErrorMessage);
        }
    }

    [Fact]
    public async Task Attempt_WhenNoFaceDetected_ReturnsGracefulResponse_AndCreatesFailedRecognitionLog()
    {
        // Custom face detection service returning FaceDetected = false
        var (client, services) = CreateClientWithIsolatedDb(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IFaceDetectionService));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddScoped<IFaceDetectionService, MockNoFaceDetectionService>();
        });

        var dummyBytes = new byte[] { 1, 2, 3 };
        using var content = new ByteArrayContent(dummyBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var response = await client.PostAsync("/api/recognition/attempt", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("No face detected in image.", result.Message);

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var log = await db.RecognitionLogs.SingleAsync();
            Assert.False(log.IsSuccess);
            Assert.Equal("No face detected in image.", log.ErrorMessage);
        }
    }

    [Fact]
    public async Task Attempt_WhenMatchedEmployeeIsInactive_RejectsAttendance_AndCreatesFailedLog()
    {
        var (client, services) = CreateClientWithIsolatedDb();
        var imageBytes = new byte[] { 50, 60, 70, 80 };

        var extractor = new DummyFaceEmbeddingExtractor();
        var embedding = await extractor.ExtractEmbeddingAsync(imageBytes);

        Guid sessionId;
        Guid employeeId;

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var employee = new Employee("EMP009", "Inactive Bob", isAdmin: false);
            employee.AddFaceEmbedding(embedding);
            employee.Deactivate(); // Set to Inactive
            db.Employees.Add(employee);

            var roster = new List<(Guid, string, string)> { (employee.Id, employee.EmployeeCode, employee.FullName) };
            var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
            db.AttendanceSessions.Add(session);

            await db.SaveChangesAsync();
            sessionId = session.Id;
            employeeId = employee.Id;
        }

        using var content = new ByteArrayContent(imageBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        var response = await client.PostAsync($"/api/recognition/attempt?sessionId={sessionId}", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal("Employee is inactive and cannot mark attendance.", result.Message);

        // Verify DB: AttendanceEntry status is STILL Absent
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.AttendanceSessions.Include(s => s.Entries).SingleAsync(s => s.Id == sessionId);
            var entry = Assert.Single(session.Entries);
            Assert.Equal(AttendanceStatus.Absent, entry.Status);
            Assert.Null(entry.MarkedAt);

            // Verify failed recognition log
            var log = await db.RecognitionLogs.SingleAsync();
            Assert.False(log.IsSuccess);
            Assert.Equal(employeeId, log.EmployeeId);
            Assert.Equal("EMP009", log.MatchedEmployeeCode);
            Assert.Equal("Matched employee is inactive.", log.ErrorMessage);
        }
    }

    [Fact]
    public async Task DeviceFlow_SimulateFullLifecycle_MarkOnce_MarkDuplicate_MarkBadImage()
    {
        var (client, services) = CreateClientWithIsolatedDb();
        var validImageBytes = new byte[] { 101, 102, 103 };

        var extractor = new DummyFaceEmbeddingExtractor();
        var embedding = await extractor.ExtractEmbeddingAsync(validImageBytes);

        Guid sessionId;

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var employee = new Employee("EMP100", "Charlie Chaplin", isAdmin: false);
            employee.AddFaceEmbedding(embedding);
            db.Employees.Add(employee);

            var roster = new List<(Guid, string, string)> { (employee.Id, employee.EmployeeCode, employee.FullName) };
            var session = AttendanceSession.Create("Production", DateOnly.FromDateTime(DateTime.Today), roster);
            db.AttendanceSessions.Add(session);

            await db.SaveChangesAsync();
            sessionId = session.Id;
        }

        // 1. Mark once (success)
        using (var content1 = new ByteArrayContent(validImageBytes))
        {
            content1.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            var res1 = await client.PostAsync($"/api/recognition/attempt?sessionId={sessionId}", content1);
            Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
            var body1 = await res1.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
            Assert.True(body1!.Success);
            Assert.Equal("Attendance marked successfully.", body1.Message);
        }

        // 2. Mark again (duplicate handled correctly)
        using (var content2 = new ByteArrayContent(validImageBytes))
        {
            content2.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            var res2 = await client.PostAsync($"/api/recognition/attempt?sessionId={sessionId}", content2);
            Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
            var body2 = await res2.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
            Assert.True(body2!.Success);
            Assert.Equal("Attendance already marked.", body2.Message);
        }

        // 3. Mark bad image (failure handled gracefully)
        using (var badContent = new ByteArrayContent([]))
        {
            badContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            var res3 = await client.PostAsync($"/api/recognition/attempt?sessionId={sessionId}", badContent);
            Assert.Equal(HttpStatusCode.BadRequest, res3.StatusCode);
            var body3 = await res3.Content.ReadFromJsonAsync<RecognitionAttemptTestResponse>();
            Assert.False(body3!.Success);
        }

        // Verify database final state
        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var session = await db.AttendanceSessions.Include(s => s.Entries).SingleAsync(s => s.Id == sessionId);
            var entry = Assert.Single(session.Entries);
            Assert.Equal(AttendanceStatus.Present, entry.Status);

            var logs = await db.RecognitionLogs.ToListAsync();
            Assert.Equal(3, logs.Count);
            Assert.Equal(2, logs.Count(l => l.IsSuccess));
            Assert.Equal(1, logs.Count(l => !l.IsSuccess));
        }
    }

    private class MockNoFaceDetectionService : IFaceDetectionService
    {
        public Task<FaceDetectionResult> DetectFacesAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FaceDetectionResult(FaceDetected: false, FaceCount: 0));
        }
    }

    private record RecognitionAttemptTestResponse(
        bool Success,
        string Message,
        Guid? EmployeeId,
        string? EmployeeCode,
        string? EmployeeName,
        DateTimeOffset? MarkedAt,
        double Confidence,
        Guid? RecognitionLogId);
}
