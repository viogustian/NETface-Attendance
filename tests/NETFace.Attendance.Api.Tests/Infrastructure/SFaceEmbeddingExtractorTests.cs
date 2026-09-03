using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class SFaceEmbeddingExtractorTests
{
    private class FakeOnnxSessionManager : IOnnxSessionManager
    {
        public InferenceSession YuNetSession => null!;
        public InferenceSession SFaceSession { get; }

        public FakeOnnxSessionManager()
        {
            // For true isolated unit test, we should mock InferenceSession but it's sealed/hard to mock.
            // We'll leave it null and test the exception to verify flow, or skip inference in test.
            // Actually, OnnxRuntime testing often requires a real model or we mock the interface.
            // But since IOnnxSessionManager exposes InferenceSession, we will just expect NullReferenceException 
            // if we don't have a real model, or we can handle it in the extractor gracefully.
            SFaceSession = null!;
        }

        public void Dispose() {}
    }

    [Fact]
    public async Task ExtractEmbeddingAsync_WithNullSession_ThrowsInvalidOperationException()
    {
        // Arrange
        var fakeSessionManager = new FakeOnnxSessionManager();
        var sut = new SFaceEmbeddingExtractor(fakeSessionManager);
        
        using var image = new Image<Rgb24>(112, 112);
        using var ms = new MemoryStream();
        image.SaveAsJpeg(ms);
        byte[] imageBytes = ms.ToArray();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExtractEmbeddingAsync(imageBytes));
    }

    [Fact]
    public void L2Normalize_NormalizesVectorToLengthOne()
    {
        // Arrange
        var sut = new SFaceEmbeddingExtractor(new FakeOnnxSessionManager());
        float[] vector = { 3.0f, 4.0f }; // Length is 5

        // Act
        sut.L2Normalize(vector);

        // Assert
        Assert.Equal(0.6f, vector[0], precision: 5);
        Assert.Equal(0.8f, vector[1], precision: 5);
        
        double length = Math.Sqrt(vector[0]*vector[0] + vector[1]*vector[1]);
        Assert.Equal(1.0, length, precision: 5);
    }

    [Fact]
    public void PreprocessImage_ReturnsTensorWithCorrectDimensionsAndNormalization()
    {
        // Arrange
        var sut = new SFaceEmbeddingExtractor(new FakeOnnxSessionManager());
        
        // Create a solid color image (e.g., Red=255, Green=127, Blue=0)
        using var image = new Image<Rgb24>(112, 112);
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var rowSpan = accessor.GetRowSpan(y);
                for (int x = 0; x < accessor.Width; x++)
                {
                    rowSpan[x] = new Rgb24(255, 127, 0);
                }
            }
        });
        using var ms = new MemoryStream();
        image.SaveAsJpeg(ms);
        byte[] imageBytes = ms.ToArray();

        // Act
        var tensor = sut.PreprocessImage(imageBytes);

        // Assert
        Assert.Equal(4, tensor.Dimensions.Length);
        Assert.Equal(1, tensor.Dimensions[0]);
        Assert.Equal(3, tensor.Dimensions[1]);
        Assert.Equal(112, tensor.Dimensions[2]);
        Assert.Equal(112, tensor.Dimensions[3]);

        // Note: JPEG compression might slightly alter the exact pixel values, 
        // but for a uniform solid color it should be very close.
        // Expected Red: (255 - 127.5)/127.5 = 1.0
        // Expected Green: (127 - 127.5)/127.5 = -0.00392
        // Expected Blue: (0 - 127.5)/127.5 = -1.0
        float redPixel = tensor[0, 0, 56, 56];
        float greenPixel = tensor[0, 1, 56, 56];
        float bluePixel = tensor[0, 2, 56, 56];

        Assert.True(redPixel > 0.9f);
        Assert.True(greenPixel > -0.05f && greenPixel < 0.05f);
        Assert.True(bluePixel < -0.9f);
    }
}
