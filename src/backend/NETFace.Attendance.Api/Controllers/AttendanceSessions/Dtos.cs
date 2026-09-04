namespace NETFace.Attendance.Api.Controllers.AttendanceSessions;

public record EmployeeRosterItem(Guid EmployeeId, string EmployeeCode, string EmployeeName);

public record CreateAttendanceSessionRequest(
    string DepartmentName,
    List<EmployeeRosterItem> Employees);

public record AttendanceEntryResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Status,
    DateTimeOffset? ClockInTime = null,
    DateTimeOffset? ClockOutTime = null,
    double? TotalWorkHours = null);

public record AttendanceSessionResponse(
    Guid Id,
    string DepartmentName,
    string Date,
    string Status,
    List<AttendanceEntryResponse> Entries);
