using System;

namespace NETFace.Attendance.Domain.Entities;

public enum EnrollmentAction
{
    FACE_ENROLLMENT,
    FACE_CLEAR
}

public class EnrollmentLog
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public EnrollmentAction Action { get; private set; }
    public bool IsSuccess { get; private set; }
    public int NumberOfPhotos { get; private set; }
    public int NumberOfEmbeddingsCreatedOrRemoved { get; private set; }
    public string PerformedBy { get; private set; }
    public string? FailureReason { get; private set; }

    // EF Core constructor
    private EnrollmentLog() { PerformedBy = string.Empty; }

    private EnrollmentLog(
        Guid employeeId,
        EnrollmentAction action,
        bool isSuccess,
        int numberOfPhotos,
        int numberOfEmbeddingsCreatedOrRemoved,
        string performedBy,
        string? failureReason)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Timestamp = DateTimeOffset.UtcNow;
        Action = action;
        IsSuccess = isSuccess;
        NumberOfPhotos = numberOfPhotos;
        NumberOfEmbeddingsCreatedOrRemoved = numberOfEmbeddingsCreatedOrRemoved;
        PerformedBy = performedBy;
        FailureReason = failureReason;
    }

    public static EnrollmentLog CreateSuccess(
        Guid employeeId,
        EnrollmentAction action,
        int numberOfPhotos,
        int numberOfEmbeddingsCreatedOrRemoved,
        string performedBy)
    {
        return new EnrollmentLog(employeeId, action, true, numberOfPhotos, numberOfEmbeddingsCreatedOrRemoved, performedBy, null);
    }

    public static EnrollmentLog CreateFailed(
        Guid employeeId,
        EnrollmentAction action,
        int numberOfPhotos,
        string performedBy,
        string failureReason)
    {
        return new EnrollmentLog(employeeId, action, false, numberOfPhotos, 0, performedBy, failureReason);
    }
}
