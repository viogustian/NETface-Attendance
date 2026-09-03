using System;
using System.Collections.Generic;
using NETFace.Attendance.Domain.Entities;

namespace NETFace.Attendance.Application.Interfaces;

public interface IFaceMatchingService
{
    double CalculateDistance(float[] vectorA, float[] vectorB);
    FaceMatchResult Match(float[] vectorA, float[] vectorB, double? threshold = null);
    FaceMatchResult FindBestMatch(float[] targetVector, IEnumerable<FaceEmbedding> candidates, double? threshold = null);
}
