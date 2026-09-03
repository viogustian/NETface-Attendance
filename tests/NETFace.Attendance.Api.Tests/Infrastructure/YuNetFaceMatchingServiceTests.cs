using System;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using SixLabors.ImageSharp;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class YuNetFaceMatchingServiceTests
{
    private class FakeOnnxSessionManager : IOnnxSessionManager
    {
        public Microsoft.ML.OnnxRuntime.InferenceSession YuNetSession => null!;
        public Microsoft.ML.OnnxRuntime.InferenceSession SFaceSession => null!;
        public void Dispose() {}
    }

    private readonly IOnnxSessionManager _fakeSessionManager;
    private readonly IFaceMatchingService _sut;

    public YuNetFaceMatchingServiceTests()
    {
        _fakeSessionManager = new FakeOnnxSessionManager();
        _sut = new YuNetFaceMatchingService(_fakeSessionManager);
    }

    [Fact]
    public void CalculateDistance_WithIdenticalVectors_ReturnsZero()
    {
        // Arrange
        float[] vectorA = { 0.1f, 0.5f, 0.9f };
        float[] vectorB = { 0.1f, 0.5f, 0.9f };

        // Act
        double distance = _sut.CalculateDistance(vectorA, vectorB);

        // Assert
        Assert.Equal(0.0, distance, precision: 5);
    }

    [Fact]
    public void Match_WithConfigurableThreshold_AppliesThresholdAccurately()
    {
        // Arrange
        // Distance is sqrt(0.3^2 + 0.4^2) = 0.5
        float[] vectorA = { 0.0f, 0.0f };
        float[] vectorB = { 0.3f, 0.4f };

        // Act & Assert with explicit threshold override
        var matchStrict = _sut.Match(vectorA, vectorB, threshold: 0.4);
        var matchRelaxed = _sut.Match(vectorA, vectorB, threshold: 0.6);

        Assert.False(matchStrict.IsMatch);
        Assert.Equal(0.5, matchStrict.Distance, precision: 5);

        Assert.True(matchRelaxed.IsMatch);
        Assert.Equal(0.5, matchRelaxed.Distance, precision: 5);
    }

    [Fact]
    public void DetectFace_WithNullSession_ThrowsInvalidOperationException()
    {
        // Arrange
        byte[] dummyImage = CreateDummyJpeg();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _sut.DetectFace(dummyImage));
    }

    private byte[] CreateDummyJpeg()
    {
        // Minimal 1x1 image in ImageSharp format
        using var image = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>(1, 1);
        using var ms = new System.IO.MemoryStream();
        image.SaveAsJpeg(ms);
        return ms.ToArray();
    }
}
