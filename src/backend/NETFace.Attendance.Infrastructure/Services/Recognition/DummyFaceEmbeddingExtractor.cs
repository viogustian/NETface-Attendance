using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using NETFace.Attendance.Application.Interfaces;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class DummyFaceEmbeddingExtractor : IFaceEmbeddingExtractor
{
    private const int EmbeddingDimension = 128;

    public Task<float[]> ExtractEmbeddingAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(imageBytes);

        if (imageBytes.Length == 0)
        {
            throw new ArgumentException("Image data cannot be empty.", nameof(imageBytes));
        }

        // Generate a deterministic 128-d normalized float vector from image bytes for consistent testing
        byte[] hash = SHA256.HashData(imageBytes);
        float[] embedding = new float[EmbeddingDimension];

        for (int i = 0; i < EmbeddingDimension; i++)
        {
            byte b = hash[i % hash.Length];
            embedding[i] = (float)b / byte.MaxValue;
        }

        return Task.FromResult(embedding);
    }
}
