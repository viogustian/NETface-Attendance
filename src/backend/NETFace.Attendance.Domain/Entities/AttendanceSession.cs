using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Domain.Exceptions;

namespace NETFace.Attendance.Domain.Entities;

public class AttendanceSession
{
    private readonly List<AttendanceEntry> _entries = [];

    public Guid Id { get; private set; }
    public string DepartmentName { get; private set; }
    public DateOnly Date { get; private set; }
    public AttendanceSessionStatus Status { get; private set; }
    public IReadOnlyList<AttendanceEntry> Entries => _entries.AsReadOnly();

    // EF Core constructor
    private AttendanceSession()
    {
        Id = Guid.NewGuid();
        DepartmentName = string.Empty;
    }

    private AttendanceSession(
        string departmentName,
        DateOnly date,
        IEnumerable<(Guid EmployeeId, string EmployeeCode, string EmployeeName)> roster)
    {
        Id = Guid.NewGuid();
        DepartmentName = departmentName;
        Date = date;
        Status = AttendanceSessionStatus.Active;

        foreach (var (employeeId, employeeCode, employeeName) in roster)
        {
            _entries.Add(new AttendanceEntry(employeeId, employeeCode, employeeName));
        }
    }

    public static AttendanceSession Create(
        string departmentName,
        DateOnly date,
        IEnumerable<(Guid EmployeeId, string EmployeeCode, string EmployeeName)> roster)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(departmentName);
        return new AttendanceSession(departmentName, date, roster);
    }

    public void FinalizeSession()
    {
        if (Status == AttendanceSessionStatus.Finalized)
            throw new AttendanceSessionAlreadyFinalizedException();

        Status = AttendanceSessionStatus.Finalized;
    }

    public void Cancel()
    {
        if (Status == AttendanceSessionStatus.Cancelled)
            return;

        if (Status == AttendanceSessionStatus.Finalized)
            throw new AttendanceSessionAlreadyFinalizedException();

        Status = AttendanceSessionStatus.Cancelled;
    }

    public bool MarkAttendance(Guid employeeId, DateTimeOffset markedAt)
    {
        if (Status == AttendanceSessionStatus.Finalized)
            throw new AttendanceSessionAlreadyFinalizedException();

        if (Status != AttendanceSessionStatus.Active)
            throw new InvalidOperationException($"Cannot mark attendance on a session with status '{Status}'.");

        var entry = _entries.FirstOrDefault(e => e.EmployeeId == employeeId);
        if (entry is null)
            return false;

        if (entry.Status == AttendanceStatus.Present)
            return false;

        entry.MarkPresent(markedAt);
        return true;
    }
}

