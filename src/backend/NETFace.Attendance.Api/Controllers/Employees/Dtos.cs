namespace NETFace.Attendance.Api.Controllers.Employees;

public record CreateEmployeeRequest(string EmployeeCode, string FullName, bool IsAdmin);

public record EmployeeResponse(Guid Id, string EmployeeCode, string FullName, bool IsAdmin, string Status);
