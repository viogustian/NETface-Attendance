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
using SixLabors.ImageSharp.Processing;

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
        // Note: For unit testing purposes if InferenceSession is null (mocked incorrectly) we handle it gracefully
        if (_sessionManager.YuNetSession == null)
        {
            throw new InvalidOperationException("YuNet Session is not initialized.");
        }

        using var image = Image.Load<Rgb24>(imageBytes);

        var inputMeta = _sessionManager.YuNetSession.InputMetadata.Values.First();
        int expectedHeight = inputMeta.Dimensions[2] > 0 ? inputMeta.Dimensions[2] : image.Height;
        int expectedWidth = inputMeta.Dimensions[3] > 0 ? inputMeta.Dimensions[3] : image.Width;

        if (image.Width != expectedWidth || image.Height != expectedHeight)
        {
            image.Mutate(x => x.Resize(expectedWidth, expectedHeight));
        }

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



        if (_sessionManager.InferenceThrottle != null)
        {
            await _sessionManager.InferenceThrottle.WaitAsync(cancellationToken);
        }

        IDisposableReadOnlyCollection<DisposableNamedOnnxValue>? results = null;
        try
        {
            results = _sessionManager.YuNetSession.Run(inputs);

            var strides = new[] { 8, 16, 32 };
            var candidates = new List<(float Score, float[] BBox, float[][] Landmarks)>();

            foreach (var stride in strides)
            {
                var cls = results.First(v => v.Name == $"cls_{stride}").AsTensor<float>();
                var obj = results.First(v => v.Name == $"obj_{stride}").AsTensor<float>();
                var bbox = results.First(v => v.Name == $"bbox_{stride}").AsTensor<float>();
                var kps = results.First(v => v.Name == $"kps_{stride}").AsTensor<float>();

                int gridW = width / stride;
                int gridH = height / stride;

                for (int y = 0; y < gridH; y++)
                {
                    for (int x = 0; x < gridW; x++)
                    {
                        int idx = y * gridW + x;
                        
                        float clsScore = cls.GetValue(idx);
                        float objScore = obj.GetValue(idx);
                        
                        // Sigmoid if needed, but assuming they are probabilities based on my test
                        float conf = clsScore * objScore;
                        
                        if (conf > 0.6f)
                        {
                            float dx = bbox.GetValue(idx * 4 + 0);
                            float dy = bbox.GetValue(idx * 4 + 1);
                            float dw = bbox.GetValue(idx * 4 + 2);
                            float dh = bbox.GetValue(idx * 4 + 3);

                            float cx = (x * stride) + dx * stride;
                            float cy = (y * stride) + dy * stride;
                            float w = (float)(Math.Exp(dw) * stride);
                            float h = (float)(Math.Exp(dh) * stride);

                            float x1 = cx - w / 2;
                            float y1 = cy - h / 2;

                            var kpsArr = new float[5][];
                            for (int k = 0; k < 5; k++)
                            {
                                float kx = kps.GetValue(idx * 10 + k * 2 + 0);
                                float ky = kps.GetValue(idx * 10 + k * 2 + 1);
                                kpsArr[k] = new float[] 
                                {
                                    (x * stride) + kx * stride, 
                                    (y * stride) + ky * stride 
                                };
                            }

                            candidates.Add((conf, new float[] { x1, y1, w, h }, kpsArr));
                        }
                    }
                }
            }

            // NMS (Non-Maximum Suppression) to filter overlapping boxes
            candidates = candidates.OrderByDescending(c => c.Score).ToList();
            var finalFaces = new List<(float Score, float[] BBox, float[][] Landmarks)>();

            foreach (var cand in candidates)
            {
                bool keep = true;
                foreach (var face in finalFaces)
                {
                    float iou = ComputeIoU(cand.BBox, face.BBox);
                    if (iou > 0.5f) // NMS threshold
                    {
                        keep = false;
                        break;
                    }
                }
                if (keep) finalFaces.Add(cand);
            }

            if (finalFaces.Count > 1)
            {
                return new FaceDetectionResult(true, finalFaces.Count) 
                {
                    Success = false,
                    ErrorCode = "multi_face_detected",
                    ErrorMessage = "Terdeteksi lebih dari satu wajah — pastikan hanya satu orang di depan kamera"
                };
            }
            
            if (finalFaces.Count == 0)
            {
                return new FaceDetectionResult(false, 0)
                {
                    Success = false,
                    ErrorCode = "no_face_detected",
                    ErrorMessage = "Wajah tidak ditemukan. Pastikan wajah terlihat jelas."
                };
            }

            var bestFace = finalFaces[0];
            return new FaceDetectionResult(true, 1)
            {
                Success = true,
                BoundingBox = bestFace.BBox,
                Landmarks = bestFace.Landmarks
            };
        }
        finally
        {
            results?.Dispose();
            _sessionManager.InferenceThrottle?.Release();
        }
    }

    private float ComputeIoU(float[] boxA, float[] boxB)
    {
        float xA = Math.Max(boxA[0], boxB[0]);
        float yA = Math.Max(boxA[1], boxB[1]);
        float xB = Math.Min(boxA[0] + boxA[2], boxB[0] + boxB[2]);
        float yB = Math.Min(boxA[1] + boxA[3], boxB[1] + boxB[3]);

        float interArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
        float boxAArea = boxA[2] * boxA[3];
        float boxBArea = boxB[2] * boxB[3];
        
        return interArea / (boxAArea + boxBArea - interArea + 1e-5f);
    }
}
