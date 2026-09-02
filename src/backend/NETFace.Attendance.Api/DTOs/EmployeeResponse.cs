using System;

namespace NETFace.Attendance.Api.DTOs;

public record EmployeeResponse(
    Guid Id,
    string EmployeeCode,
    string ProfileDetails,
    string Status,
    bool AdminFlag,
    int EmbeddingsCount
);
