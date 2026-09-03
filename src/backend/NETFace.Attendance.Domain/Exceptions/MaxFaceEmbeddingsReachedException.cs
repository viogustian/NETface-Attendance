namespace NETFace.Attendance.Domain.Exceptions;

public class MaxFaceEmbeddingsReachedException : Exception
{
    public MaxFaceEmbeddingsReachedException()
        : base("An employee cannot have more than 5 face embeddings.")
    {
    }
}
