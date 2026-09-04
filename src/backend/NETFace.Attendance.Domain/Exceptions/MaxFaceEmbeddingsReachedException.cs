namespace NETFace.Attendance.Domain.Exceptions;

public class MaxFaceEmbeddingsReachedException : DomainException
{
    public MaxFaceEmbeddingsReachedException(string message) : base(message)
    {
    }
}
