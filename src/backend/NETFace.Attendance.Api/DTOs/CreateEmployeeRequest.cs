namespace NETFace.Attendance.Api.DTOs;

public record CreateEmployeeRequest(string EmployeeCode, string ProfileDetails, bool AdminFlag);
