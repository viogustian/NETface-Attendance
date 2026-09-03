using System;

namespace NETFace.Attendance.Application.Interfaces;

public record FaceMatchResult(
    bool IsMatch,
    double Distance,
    Guid? MatchedEmployeeId = null);
