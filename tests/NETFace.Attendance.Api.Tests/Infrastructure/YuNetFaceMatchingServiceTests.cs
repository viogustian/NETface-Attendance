using System;
using System.Collections.Generic;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;
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
        public System.Threading.SemaphoreSlim InferenceThrottle { get; } = new(1);
        public void Dispose() {}
    }

    private class FakeOptionsMonitor : Microsoft.Extensions.Options.IOptionsMonitor<FaceMatchingOptions>
    {
        public FaceMatchingOptions CurrentValue { get; set; } = new FaceMatchingOptions { MatchThreshold = 0.5 };
        public FaceMatchingOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<FaceMatchingOptions, string?> listener) => null;
    }

    private readonly IOnnxSessionManager _fakeSessionManager;
    private readonly Microsoft.Extensions.Options.IOptionsMonitor<FaceMatchingOptions> _fakeOptionsMonitor;
    private readonly IFaceMatchingService _sut;

    public YuNetFaceMatchingServiceTests()
    {
        _fakeSessionManager = new FakeOnnxSessionManager();
        _fakeOptionsMonitor = new FakeOptionsMonitor();
        _sut = new YuNetFaceMatchingService(_fakeSessionManager, _fakeOptionsMonitor);
    }

    [Fact]
    public void CalculateDistance_WithIdenticalVectors_ReturnsZero()
    {
        // Arrange (L2 Normalized vectors)
        float[] vectorA = { 0.0f, 1.0f, 0.0f };
        float[] vectorB = { 0.0f, 1.0f, 0.0f };

        // Act
        // Dot Product = 0*0 + 1*1 + 0*0 = 1.0
        // Distance = 1.0 - 1.0 = 0.0
        double distance = _sut.CalculateDistance(vectorA, vectorB);

        // Assert
        Assert.Equal(0.0, distance, precision: 5);
    }

    [Fact]
    public void CalculateDistance_WithOrthogonalVectors_ReturnsOne()
    {
        // Arrange
        float[] vectorA = { 1.0f, 0.0f, 0.0f };
        float[] vectorB = { 0.0f, 1.0f, 0.0f };

        // Act
        // Dot Product = 0
        // Distance = 1.0 - 0 = 1.0
        double distance = _sut.CalculateDistance(vectorA, vectorB);

        // Assert
        Assert.Equal(1.0, distance, precision: 5);
    }

    [Fact]
    public void Match_WithConfigurableThreshold_AppliesThresholdAccurately()
    {
        // Arrange
        // vectorA = [1, 0], vectorB = [0.8, 0.6] (both L2 normalized)
        // Dot Product = 1*0.8 + 0*0.6 = 0.8
        // Distance = 1.0 - 0.8 = 0.2
        float[] vectorA = { 1.0f, 0.0f };
        float[] vectorB = { 0.8f, 0.6f };

        // Act & Assert with explicit threshold override
        var matchStrict = _sut.Match(vectorA, vectorB, threshold: 0.1);
        var matchRelaxed = _sut.Match(vectorA, vectorB, threshold: 0.3);

        Assert.False(matchStrict.IsMatch);
        Assert.Equal(0.2, matchStrict.Distance, precision: 5);

        Assert.True(matchRelaxed.IsMatch);
        Assert.Equal(0.2, matchRelaxed.Distance, precision: 5);
    }

    [Fact]
    public void FindBestMatch_WithClosestCandidateWithinThreshold_ReturnsBestMatch()
    {
        // Arrange
        var employee1 = new Employee("EMP-001", "Alice", false);
        employee1.AddFaceEmbedding([1.0f, 0.0f]);

        var employee2 = new Employee("EMP-002", "Bob", false);
        employee2.AddFaceEmbedding([0.0f, 1.0f]);

        var candidates = new[] { employee1, employee2 };
        float[] queryVector = [0.9f, 0.1f];

        // Act
        var result = _sut.FindBestMatch(queryVector, candidates);

        // Assert
        Assert.True(result.IsMatch);
        Assert.Equal(employee1.Id, result.MatchedEmployeeId);
        Assert.True(result.Distance < 0.2);
    }

    [Fact]
    public void FindBestMatch_WhenAllCandidatesExceedThreshold_ReturnsNoMatch()
    {
        // Arrange
        var employee1 = new Employee("EMP-001", "Alice", false);
        employee1.AddFaceEmbedding([0.0f, 1.0f]);

        var candidates = new[] { employee1 };
        float[] queryVector = [1.0f, 0.0f]; // Orthogonal, distance = 1.0 > 0.5

        // Act
        var result = _sut.FindBestMatch(queryVector, candidates);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Null(result.MatchedEmployeeId);
    }

    [Fact]
    public async Task DetectFace_WithNullSession_ThrowsInvalidOperationException()
    {
        // Arrange
        byte[] dummyImage = CreateDummyJpeg();
        var detectionService = new YuNetFaceDetectionService(new FakeOnnxSessionManager());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => detectionService.DetectFacesAsync(dummyImage));
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
