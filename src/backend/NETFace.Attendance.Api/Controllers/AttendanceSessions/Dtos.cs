namespace NETFace.Attendance.Api.Controllers.AttendanceSessions;

public record EmployeeRosterItem(Guid EmployeeId, string EmployeeCode, string EmployeeName);

public record CreateAttendanceSessionRequest(
    string DepartmentName,
    DateOnly Date,
    List<EmployeeRosterItem> Employees);

public record AttendanceEntryResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Status);

public record AttendanceSessionResponse(
    Guid Id,
    string DepartmentName,
    string Date,
    string Status,
    List<AttendanceEntryResponse> Entries);
