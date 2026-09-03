namespace NETFace.Attendance.Application.Interfaces;

public record FaceDetectionResult
{
    public bool FaceDetected { get; init; }
    public int FaceCount { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }
    public float[]? BoundingBox { get; init; }
    public float[][]? Landmarks { get; init; }

    public FaceDetectionResult(bool faceDetected, int faceCount = 1)
    {
        FaceDetected = faceDetected;
        FaceCount = faceCount;
        Success = faceDetected;
    }
}
