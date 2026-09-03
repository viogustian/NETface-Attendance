namespace NETFace.Attendance.Domain.Entities;

public class FaceEmbedding
{
    public Guid Id { get; private set; }
    public float[] Vector { get; private set; }
    public DateTimeOffset CapturedAt { get; private set; }

    // EF Core constructor
    private FaceEmbedding() { Id = Guid.NewGuid(); Vector = []; CapturedAt = DateTimeOffset.UtcNow; }

    public FaceEmbedding(float[] vector)
    {
        Id = Guid.NewGuid();
        Vector = vector;
        CapturedAt = DateTimeOffset.UtcNow;
    }
}
