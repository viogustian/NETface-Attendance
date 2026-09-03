namespace NETFace.Attendance.Domain.Exceptions;

public class AttendanceSessionAlreadyFinalizedException : Exception
{
    public AttendanceSessionAlreadyFinalizedException()
        : base("This attendance session is finalized and cannot be modified.")
    {
    }
}
