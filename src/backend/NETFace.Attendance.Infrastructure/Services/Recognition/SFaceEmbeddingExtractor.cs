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

public class SFaceEmbeddingExtractor : IFaceEmbeddingExtractor
{
    private readonly IOnnxSessionManager _sessionManager;
    private const int TargetWidth = 112;
    private const int TargetHeight = 112;

    public SFaceEmbeddingExtractor(IOnnxSessionManager sessionManager)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
    }

    public async Task<float[]> ExtractEmbeddingAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        if (_sessionManager.SFaceSession == null)
        {
            throw new InvalidOperationException("SFace Session is not initialized.");
        }

        var tensor = PreprocessImage(imageBytes);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("data", tensor)
        };

        if (_sessionManager.InferenceThrottle != null)
        {
            await _sessionManager.InferenceThrottle.WaitAsync(cancellationToken);
        }

        try
        {
            using var results = _sessionManager.SFaceSession.Run(inputs);
            var outputTensor = results.First().AsTensor<float>();
            
            float[] embedding = outputTensor.ToArray();

            // Perform L2 Normalization explicitly on C# layer
            L2Normalize(embedding);

            return embedding;
        }
        finally
        {
            _sessionManager.InferenceThrottle?.Release();
        }
    }

    public DenseTensor<float> PreprocessImage(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            throw new ArgumentException("Image bytes cannot be empty.", nameof(imageBytes));
        }

        using var image = Image.Load<Rgb24>(imageBytes);
        
        // Ensure image is 112x112
        if (image.Width != TargetWidth || image.Height != TargetHeight)
        {
            image.Mutate(x => x.Resize(TargetWidth, TargetHeight));
        }

        // Create Tensor for SFace: [1, 3, 112, 112]
        // SFace normalization: (pixel - 127.5) / 127.5
        // RGB SFace Color space constraint: No channel swap required from RGB
        // As per ADR 0003, SFace expects RGB format tensor natively.
        var tensor = new DenseTensor<float>(new[] { 1, 3, TargetHeight, TargetWidth });

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var rowSpan = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    var pixel = rowSpan[x];
                    tensor[0, 0, y, x] = (pixel.R - 127.5f) / 127.5f;
                    tensor[0, 1, y, x] = (pixel.G - 127.5f) / 127.5f;
                    tensor[0, 2, y, x] = (pixel.B - 127.5f) / 127.5f;
                }
            }
        });

        return tensor;
    }

    public void L2Normalize(float[] vector)
    {
        double sum = 0;
        for (int i = 0; i < vector.Length; i++)
        {
            sum += vector[i] * vector[i];
        }

        float magnitude = (float)Math.Sqrt(sum);
        if (magnitude > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= magnitude;
            }
        }
    }
}
