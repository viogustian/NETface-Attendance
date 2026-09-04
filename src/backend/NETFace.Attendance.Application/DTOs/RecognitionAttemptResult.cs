using System;

namespace NETFace.Attendance.Application.DTOs;

public record RecognitionAttemptResult(
    bool Success,
    string Message,
    Guid? EmployeeId = null,
    string? EmployeeCode = null,
    string? EmployeeName = null,
    DateTimeOffset? MarkedAt = null,
    double Confidence = 0.0,
    Guid? RecognitionLogId = null,
    bool FallbackToPin = false,
    bool IsConsecutiveFailure = false);
