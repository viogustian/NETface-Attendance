using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private readonly double _defaultThreshold;

    public YuNetFaceMatchingService(IOnnxSessionManager sessionManager, FaceMatchingOptions? options = null)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        _defaultThreshold = options?.MatchThreshold ?? 0.6;
    }

    public FaceDetectionResult DetectFace(byte[] imageBytes)
    {
        using var image = Image.Load<Rgb24>(imageBytes);
        int width = image.Width;
        int height = image.Height;

        // BGR YuNet Color space constraint: Channel swap from RGB to BGR
        // As per ADR 0003, YuNet expects BGR format tensor.
        var tensor = new DenseTensor<float>(new[] { 1, 3, height, width });
        
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var rowSpan = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    // RGB to BGR
                    tensor[0, 0, y, x] = rowSpan[x].B; 
                    tensor[0, 1, y, x] = rowSpan[x].G; 
                    tensor[0, 2, y, x] = rowSpan[x].R; 
                }
            }
        });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input", tensor)
        };

        // Note: For unit testing purposes if InferenceSession is null (mocked incorrectly) we handle it gracefully
        if (_sessionManager.YuNetSession == null)
        {
            throw new InvalidOperationException("YuNet Session is not initialized.");
        }

        using var results = _sessionManager.YuNetSession.Run(inputs);
        var outputTensor = results.First(v => v.Name == "dets").AsTensor<float>();
        
        int numDetections = outputTensor.Dimensions.Length > 0 ? outputTensor.Dimensions[0] : 0;
        
        if (numDetections > 1) 
        {
            return new FaceDetectionResult(true, numDetections) 
            {
                Success = false,
                ErrorCode = "multi_face_detected",
                ErrorMessage = "Terdeteksi lebih dari satu wajah — pastikan hanya satu orang di depan kamera"
            };
        }
        
        if (numDetections == 0)
        {
            return new FaceDetectionResult(false, 0)
            {
                Success = false,
                ErrorCode = "no_face_detected",
                ErrorMessage = "Tidak ada wajah"
            };
        }

        // Example indices for standard YuNet output
        float confidence = outputTensor[0, 14];
        if (confidence < 0.6f)
        {
             return new FaceDetectionResult(true, 1)
            {
                Success = false,
                ErrorCode = "no_face_detected",
                ErrorMessage = "Confidence level < 0.6"
            };
        }

        var bbox = new float[] { outputTensor[0,0], outputTensor[0,1], outputTensor[0,2], outputTensor[0,3] };
        var landmarks = new float[][] {
            new float[] { outputTensor[0,4], outputTensor[0,5] }, // right eye
            new float[] { outputTensor[0,6], outputTensor[0,7] }, // left eye
            new float[] { outputTensor[0,8], outputTensor[0,9] }, // nose
            new float[] { outputTensor[0,10], outputTensor[0,11] }, // right mouth
            new float[] { outputTensor[0,12], outputTensor[0,13] }  // left mouth
        };

        return new FaceDetectionResult(true, 1)
        {
            Success = true,
            BoundingBox = bbox,
            Landmarks = landmarks
        };
    }

    public double CalculateDistance(float[] vectorA, float[] vectorB)
    {
        // Simple euclidean for now (to satisfy interface)
        double sum = 0.0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            sum += Math.Pow(vectorA[i] - vectorB[i], 2);
        }
        return Math.Sqrt(sum);
    }

    public FaceMatchResult Match(float[] vectorA, float[] vectorB, double? threshold = null)
    {
        double distance = CalculateDistance(vectorA, vectorB);
        return new FaceMatchResult(distance <= (threshold ?? _defaultThreshold), distance);
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
}
