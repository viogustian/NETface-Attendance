using System;

namespace NETFace.Attendance.Domain.Entities;

public class RecognitionLog
{
    public Guid Id { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public string? MatchedEmployeeCode { get; private set; }
    public bool IsSuccess { get; private set; }
    public double Confidence { get; private set; }
    public long ProcessingTimeMs { get; private set; }
    public DateTimeOffset AttemptedAt { get; private set; }
    public string? ErrorMessage { get; private set; }

    // EF Core constructor
    private RecognitionLog()
    {
        Id = Guid.NewGuid();
        AttemptedAt = DateTimeOffset.UtcNow;
    }

    public static RecognitionLog CreateSuccess(
        Guid employeeId,
        string matchedEmployeeCode,
        double confidence,
        long processingTimeMs,
        DateTimeOffset attemptedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(matchedEmployeeCode);

        return new RecognitionLog
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            MatchedEmployeeCode = matchedEmployeeCode,
            IsSuccess = true,
            Confidence = confidence,
            ProcessingTimeMs = processingTimeMs,
            AttemptedAt = attemptedAt,
            ErrorMessage = null
        };
    }

    public static RecognitionLog CreateFailed(
        string errorMessage,
        long processingTimeMs,
        DateTimeOffset attemptedAt,
        Guid? employeeId = null,
        string? matchedEmployeeCode = null,
        double confidence = 0.0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return new RecognitionLog
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            MatchedEmployeeCode = matchedEmployeeCode,
            IsSuccess = false,
            Confidence = confidence,
            ProcessingTimeMs = processingTimeMs,
            AttemptedAt = attemptedAt,
            ErrorMessage = errorMessage
        };
    }
}
