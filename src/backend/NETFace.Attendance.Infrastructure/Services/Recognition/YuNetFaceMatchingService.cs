using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class YuNetFaceMatchingService : IFaceMatchingService
{
    private readonly IOnnxSessionManager _sessionManager;
    private readonly IOptionsMonitor<FaceMatchingOptions> _optionsMonitor;

    public YuNetFaceMatchingService(IOnnxSessionManager sessionManager, IOptionsMonitor<FaceMatchingOptions> optionsMonitor)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
    }



    public double CalculateDistance(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        double dotProduct = 0.0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
        }
        
        // We use 1.0 - dotProduct so that smaller distance = closer match.
        // A perfect match has a dot product of 1.0, hence distance = 0.0.
        return 1.0 - dotProduct;
    }

    public FaceMatchResult Match(float[] vectorA, float[] vectorB, double? threshold = null)
    {
        double distance = CalculateDistance(vectorA, vectorB);
        return new FaceMatchResult(distance <= (threshold ?? _optionsMonitor.CurrentValue.MatchThreshold), distance);
    }

    public FaceMatchResult FindBestMatch(float[] targetVector, IEnumerable<Employee> candidates, double? threshold = null)
    {
        ArgumentNullException.ThrowIfNull(targetVector);
        ArgumentNullException.ThrowIfNull(candidates);

        double effectiveThreshold = threshold ?? _optionsMonitor.CurrentValue.MatchThreshold;
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
}
