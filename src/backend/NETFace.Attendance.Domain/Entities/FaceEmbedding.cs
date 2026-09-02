using System;

namespace NETFace.Attendance.Domain.Entities;

public class FaceEmbedding
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public float[] Vector { get; private set; }
    public DateTimeOffset CapturedAt { get; private set; }

    // EF Core constructor
    private FaceEmbedding() { Vector = Array.Empty<float>(); }

    internal FaceEmbedding(Guid employeeId, float[] vector)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        Vector = vector ?? throw new ArgumentNullException(nameof(vector));
        CapturedAt = DateTimeOffset.UtcNow;
    }
}
