namespace NETFace.Attendance.Api.Controllers.Employees;

public record CreateEmployeeRequest(string EmployeeCode, string FullName, bool IsAdmin);

public record EmployeeResponse(Guid Id, string EmployeeCode, string FullName, bool IsAdmin, string Status, int EnrolledFacesCount);

public record FaceEnrollmentResponse(bool Success, Guid EmployeeId, int EnrolledCount, int RemainingSlots);
public record FaceClearResponse(bool Success, Guid EmployeeId, int ClearedCount, int RemainingSlots);
