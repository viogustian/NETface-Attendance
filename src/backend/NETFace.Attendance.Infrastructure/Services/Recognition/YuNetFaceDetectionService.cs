using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NETFace.Attendance.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class YuNetFaceDetectionService : IFaceDetectionService
{
    private readonly IOnnxSessionManager _sessionManager;

    public YuNetFaceDetectionService(IOnnxSessionManager sessionManager)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public async Task<FaceDetectionResult> DetectFacesAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
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

        if (_sessionManager.InferenceThrottle != null)
        {
            await _sessionManager.InferenceThrottle.WaitAsync(cancellationToken);
        }

        IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
        try
        {
            results = _sessionManager.YuNetSession.Run(inputs);
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
        finally
        {
            results?.Dispose();
            _sessionManager.InferenceThrottle?.Release();
        }
    }
}
