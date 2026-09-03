using System;
using System.Collections.Generic;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class DummyFaceMatchingService : IFaceMatchingService
{
    private readonly double _defaultThreshold;

    public DummyFaceMatchingService(FaceMatchingOptions? options = null)
    {
        _defaultThreshold = options?.MatchThreshold ?? 0.6;
    }

    public double CalculateDistance(float[] vectorA, float[] vectorB)
    {
        ArgumentNullException.ThrowIfNull(vectorA);
        ArgumentNullException.ThrowIfNull(vectorB);

        if (vectorA.Length == 0)
        {
            throw new ArgumentException("Vector cannot be empty.", nameof(vectorA));
        }

        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vector dimensions must match.", nameof(vectorB));
        }

        double sum = 0.0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            double diff = vectorA[i] - vectorB[i];
            sum += diff * diff;
        }

        return Math.Sqrt(sum);
    }

    public FaceMatchResult Match(float[] vectorA, float[] vectorB, double? threshold = null)
    {
        double distance = CalculateDistance(vectorA, vectorB);
        double effectiveThreshold = threshold ?? _defaultThreshold;
        bool isMatch = distance <= effectiveThreshold;

        return new FaceMatchResult(isMatch, distance);
    }

    public FaceMatchResult FindBestMatch(float[] targetVector, IEnumerable<Employee> candidates, double? threshold = null)
    {
        ArgumentNullException.ThrowIfNull(targetVector);
        ArgumentNullException.ThrowIfNull(candidates);

        double effectiveThreshold = threshold ?? _defaultThreshold;
        double minDistance = double.MaxValue;
        Guid? matchedEmployeeId = null;

        foreach (var employee in candidates)
        {
            foreach (var candidate in employee.FaceEmbeddings)
            {
                if (candidate?.Vector == null)
                {
                    continue;
                }

                double distance = CalculateDistance(targetVector, candidate.Vector);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    matchedEmployeeId = employee.Id;
                }
            }
        }

        bool isMatch = matchedEmployeeId.HasValue && minDistance <= effectiveThreshold;

        return new FaceMatchResult(
            isMatch,
            matchedEmployeeId.HasValue ? minDistance : double.PositiveInfinity,
            isMatch ? matchedEmployeeId : null);
    }

    public FaceDetectionResult DetectFace(byte[] imageBytes)
    {
        // Dummy implementation
        return new FaceDetectionResult(true, 1)
        {
            FaceDetected = true,
            Success = true,
            BoundingBox = new float[] { 10, 10, 100, 100 },
            Landmarks = new float[][] {
                new float[] { 30, 30 },
                new float[] { 70, 30 },
                new float[] { 50, 50 },
                new float[] { 40, 80 },
                new float[] { 60, 80 }
            }
        };
    }
}
