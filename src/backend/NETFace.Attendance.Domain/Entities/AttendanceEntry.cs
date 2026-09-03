using NETFace.Attendance.Domain.Enums;

namespace NETFace.Attendance.Domain.Entities;

public class AttendanceEntry
{
    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }

    // Snapshot fields — preserved even if employee data changes
    public string EmployeeCode { get; private set; }
    public string EmployeeName { get; private set; }

    public AttendanceStatus Status { get; private set; }
    public DateTimeOffset? MarkedAt { get; private set; }

    // EF Core constructor
    private AttendanceEntry()
    {
        Id = Guid.NewGuid();
        EmployeeCode = string.Empty;
        EmployeeName = string.Empty;
    }

    internal AttendanceEntry(Guid employeeId, string employeeCode, string employeeName)
    {
        Id = Guid.NewGuid();
        EmployeeId = employeeId;
        EmployeeCode = employeeCode;
        EmployeeName = employeeName;
        Status = AttendanceStatus.Absent;
    }

    internal void MarkPresent(DateTimeOffset markedAt)
    {
        Status = AttendanceStatus.Present;
        MarkedAt = markedAt;
    }
}

