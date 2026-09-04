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
    
    // Legacy field, keeping it for backwards compatibility during migration or just replacing it.
    // Actually, we'll keep MarkedAt as the FIRST time they were seen (Clock In), but explicitly add ClockIn/Out.
    public DateTimeOffset? MarkedAt { get; private set; }

    public DateTimeOffset? ClockInTime { get; private set; }
    public DateTimeOffset? ClockOutTime { get; private set; }
    public double? TotalWorkHours { get; private set; }

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

    internal void MarkAttendance(DateTimeOffset markedAt, TimeSpan clockOutStartTime)
    {
        // If it's before the ClockOutStartTime (using local time of markedAt for comparison, or assuming markedAt is UTC and shift is UTC)
        // A better approach: Compare the TimeOfDay of markedAt.
        // E.g., markedAt.TimeOfDay >= clockOutStartTime -> Clock Out
        
        var timeOfDay = markedAt.ToLocalTime().TimeOfDay; // Using local time for time-of-day comparison
        
        if (timeOfDay >= clockOutStartTime)
        {
            // Clock Out
            ClockOutTime = markedAt;
            Status = AttendanceStatus.Present;
            
            // If they clocked in earlier, calculate total hours
            if (ClockInTime.HasValue)
            {
                TotalWorkHours = (ClockOutTime.Value - ClockInTime.Value).TotalHours;
            }
        }
        else
        {
            // Clock In
            if (!ClockInTime.HasValue) // Only set ClockIn once
            {
                ClockInTime = markedAt;
                MarkedAt = markedAt; // For backwards compatibility
                Status = AttendanceStatus.Present;
            }
        }
    }
}

