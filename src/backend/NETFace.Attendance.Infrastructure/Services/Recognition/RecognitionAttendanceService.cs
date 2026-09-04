using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using NETFace.Attendance.Application.DTOs;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class RecognitionAttendanceService(
    AppDbContext db,
    IFaceDetectionService faceDetectionService,
    IFaceEmbeddingExtractor faceEmbeddingExtractor,
    IFaceMatchingService faceMatchingService,
    ISpoofingDetectionService? spoofingDetectionService = null,
    ILogger<RecognitionAttendanceService>? logger = null) : IRecognitionAttendanceService
{
    private readonly ISpoofingDetectionService _spoofingDetectionService = spoofingDetectionService ?? new SpoofingDetectionService();
    private readonly ILogger<RecognitionAttendanceService>? _logger = logger;

    public async Task<RecognitionAttemptResult> AttemptRecognitionAsync(
        byte[] imageBytes,
        Guid? sessionId = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        if (imageBytes == null || imageBytes.Length == 0)
        {
            stopwatch.Stop();
            return await FailAsync("Image data is required and cannot be empty.", stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        FaceDetectionResult detectionResult;
        float[] embedding;

        try
        {
            // 1. Detect face
            detectionResult = await faceDetectionService.DetectFacesAsync(imageBytes, cancellationToken);
            if (!detectionResult.FaceDetected)
            {
                stopwatch.Stop();
                return await FailAsync("No face detected in image.", stopwatch.ElapsedMilliseconds, cancellationToken);
            }

            // 2. Extract embedding
            embedding = await faceEmbeddingExtractor.ExtractEmbeddingAsync(imageBytes, cancellationToken);
        }
        catch (Exception ex) when (ex is FileNotFoundException or OutOfMemoryException or OnnxRuntimeException or InvalidOperationException)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Face recognition model failure or memory exhaustion. Falling back to PIN mode.");

            var fallbackLog = RecognitionLog.CreateFailed(
                $"Fallback to PIN mode due to recognition failure: {ex.Message}",
                stopwatch.ElapsedMilliseconds,
                DateTimeOffset.UtcNow);

            db.RecognitionLogs.Add(fallbackLog);
            await db.SaveChangesAsync(cancellationToken);

            return new RecognitionAttemptResult(
                Success: false,
                Message: "Face recognition unavailable. Silakan gunakan PIN mode untuk absensi.",
                FallbackToPin: true,
                RecognitionLogId: fallbackLog.Id);
        }

        // 3. Resolve active session
        AttendanceSession? session;
        if (sessionId.HasValue)
        {
            session = await db.AttendanceSessions
                .Include(s => s.Entries)
                .FirstOrDefaultAsync(s => s.Id == sessionId.Value, cancellationToken);
        }
        else
        {
            session = await db.AttendanceSessions
                .Include(s => s.Entries)
                .FirstOrDefaultAsync(s => s.Status == AttendanceSessionStatus.Active, cancellationToken);
        }

        if (session == null)
        {
            stopwatch.Stop();
            return await FailAsync("No active attendance session found.", stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        if (session.Status != AttendanceSessionStatus.Active)
        {
            stopwatch.Stop();
            return await FailAsync(
                $"Attendance session is not active (status: {session.Status}).",
                stopwatch.ElapsedMilliseconds,
                cancellationToken,
                logErrorMessage: $"Attendance session is {session.Status} and not active.");
        }

        // 4. Match against employees
        var employees = await db.Employees
            .Include(e => e.FaceEmbeddings)
            .ToListAsync(cancellationToken);

        var matchResult = faceMatchingService.FindBestMatch(embedding, employees);
        string trackingKey = sessionId?.ToString() ?? "default-terminal";

        if (!matchResult.IsMatch || !matchResult.MatchedEmployeeId.HasValue)
        {
            int failureCount = _spoofingDetectionService.RecordUnknownFace(trackingKey);
            stopwatch.Stop();

            if (failureCount >= 3)
            {
                string auditMessage = $"Audit Alert: Consecutive unknown face attempts detected ({failureCount}) — possible spoofing.";
                _logger?.LogWarning("AUDIT ALERT [SPOOFING]: Consecutive unknown face attempts ({Count}) detected for key {Key}. Access denied with 401.", failureCount, trackingKey);

                var spoofingLog = RecognitionLog.CreateFailed(
                    auditMessage,
                    stopwatch.ElapsedMilliseconds,
                    DateTimeOffset.UtcNow);

                db.RecognitionLogs.Add(spoofingLog);
                await db.SaveChangesAsync(cancellationToken);

                return new RecognitionAttemptResult(
                    Success: false,
                    Message: "Access Denied — Consecutive unknown faces detected (possible spoofing).",
                    RecognitionLogId: spoofingLog.Id,
                    IsConsecutiveFailure: true);
            }

            return await FailAsync("No matching employee found.", stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        var matchedEmployee = employees.FirstOrDefault(e => e.Id == matchResult.MatchedEmployeeId.Value);
        if (matchedEmployee == null)
        {
            stopwatch.Stop();
            return await FailAsync("Matched employee not found.", stopwatch.ElapsedMilliseconds, cancellationToken);
        }

        double confidence = Math.Round(Math.Max(0.0, 1.0 - matchResult.Distance), 4);

        // 5. Inactive employee rejection with high-priority alert
        if (matchedEmployee.Status == EmployeeStatus.Inactive)
        {
            stopwatch.Stop();
            _logger?.LogWarning(
                "HIGH-PRIORITY SECURITY ALERT: Access Denied — Inactive Employee {EmployeeCode} ({EmployeeId}, {FullName}) attempted recognition with confidence {Confidence}.",
                matchedEmployee.EmployeeCode, matchedEmployee.Id, matchedEmployee.FullName, confidence);

            return await FailAsync(
                "Access Denied — Inactive Employee",
                stopwatch.ElapsedMilliseconds,
                cancellationToken,
                employeeId: matchedEmployee.Id,
                employeeCode: matchedEmployee.EmployeeCode,
                employeeName: matchedEmployee.FullName,
                confidence: confidence,
                logErrorMessage: "Access Denied — Inactive Employee");
        }

        // 6. Check session roster
        var entry = session.Entries.FirstOrDefault(e => e.EmployeeId == matchedEmployee.Id);
        if (entry == null)
        {
            stopwatch.Stop();
            return await FailAsync(
                "Employee is not registered in this session roster.",
                stopwatch.ElapsedMilliseconds,
                cancellationToken,
                employeeId: matchedEmployee.Id,
                employeeCode: matchedEmployee.EmployeeCode,
                employeeName: matchedEmployee.FullName,
                confidence: confidence);
        }

        // Recognition succeeded -> reset consecutive unknown face counter
        _spoofingDetectionService.RecordSuccess(trackingKey);

        // 7. Duplicate recognition check
        if (entry.Status == AttendanceStatus.Present)
        {
            stopwatch.Stop();
            var log = RecognitionLog.CreateSuccess(
                matchedEmployee.Id,
                matchedEmployee.EmployeeCode,
                confidence,
                stopwatch.ElapsedMilliseconds,
                DateTimeOffset.UtcNow);

            db.RecognitionLogs.Add(log);
            await db.SaveChangesAsync(cancellationToken);

            return new RecognitionAttemptResult(
                Success: true,
                Message: "Attendance already marked.",
                EmployeeId: matchedEmployee.Id,
                EmployeeCode: matchedEmployee.EmployeeCode,
                EmployeeName: entry.EmployeeName,
                MarkedAt: entry.MarkedAt,
                Confidence: confidence,
                RecognitionLogId: log.Id);
        }

        // 8. First valid recognition: mark as Present
        var now = DateTimeOffset.UtcNow;
        session.MarkAttendance(matchedEmployee.Id, now);

        stopwatch.Stop();
        var successLog = RecognitionLog.CreateSuccess(
            matchedEmployee.Id,
            matchedEmployee.EmployeeCode,
            confidence,
            stopwatch.ElapsedMilliseconds,
            now);

        db.RecognitionLogs.Add(successLog);
        await db.SaveChangesAsync(cancellationToken);

        return new RecognitionAttemptResult(
            Success: true,
            Message: "Attendance marked successfully.",
            EmployeeId: matchedEmployee.Id,
            EmployeeCode: matchedEmployee.EmployeeCode,
            EmployeeName: entry.EmployeeName,
            MarkedAt: now,
            Confidence: confidence,
            RecognitionLogId: successLog.Id);
    }

    private async Task<RecognitionAttemptResult> FailAsync(
        string message,
        long elapsedMs,
        CancellationToken cancellationToken,
        Guid? employeeId = null,
        string? employeeCode = null,
        string? employeeName = null,
        double confidence = 0.0,
        string? logErrorMessage = null)
    {
        var log = RecognitionLog.CreateFailed(
            logErrorMessage ?? message,
            elapsedMs,
            DateTimeOffset.UtcNow,
            employeeId: employeeId,
            matchedEmployeeCode: employeeCode,
            confidence: confidence);

        db.RecognitionLogs.Add(log);
        await db.SaveChangesAsync(cancellationToken);

        return new RecognitionAttemptResult(
            Success: false,
            Message: message,
            EmployeeId: employeeId,
            EmployeeCode: employeeCode,
            EmployeeName: employeeName,
            Confidence: confidence,
            RecognitionLogId: log.Id);
    }
}
