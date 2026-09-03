using System;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class DummyFaceMatchingServiceTests
{
    private readonly IFaceMatchingService _sut = new DummyFaceMatchingService();

    [Fact]
    public void CalculateDistance_WithIdenticalVectors_ReturnsZero()
    {
        // Arrange
        float[] vectorA = [0.1f, 0.5f, 0.9f];
        float[] vectorB = [0.1f, 0.5f, 0.9f];

        // Act
        double distance = _sut.CalculateDistance(vectorA, vectorB);

        // Assert
        Assert.Equal(0.0, distance, precision: 5);
    }

    [Fact]
    public void CalculateDistance_WithKnownVectors_ReturnsExpectedEuclideanDistance()
    {
        // Arrange
        // Distance between (0, 0) and (3, 4) in 2D space is sqrt(3^2 + 4^2) = 5
        float[] vectorA = [0.0f, 0.0f];
        float[] vectorB = [3.0f, 4.0f];

        // Act
        double distance = _sut.CalculateDistance(vectorA, vectorB);

        // Assert
        Assert.Equal(5.0, distance, precision: 5);
    }

    [Fact]
    public void Match_WithIdenticalVectors_ReturnsMatchTrue()
    {
        // Arrange
        float[] vectorA = [0.2f, 0.4f, 0.6f];
        float[] vectorB = [0.2f, 0.4f, 0.6f];

        // Act
        var result = _sut.Match(vectorA, vectorB);

        // Assert
        Assert.True(result.IsMatch);
        Assert.Equal(0.0, result.Distance, precision: 5);
    }

    [Fact]
    public void Match_WithDrasticallyDifferentVectors_ReturnsMatchFalse()
    {
        // Arrange
        float[] vectorA = [0.0f, 0.0f, 0.0f];
        float[] vectorB = [1.0f, 1.0f, 1.0f]; // Distance is sqrt(3) ~ 1.732 > 0.6 default threshold

        // Act
        var result = _sut.Match(vectorA, vectorB);

        // Assert
        Assert.False(result.IsMatch);
        Assert.True(result.Distance > 0.6);
    }

    [Fact]
    public void Match_WithConfigurableThreshold_AppliesThresholdAccurately()
    {
        // Arrange
        // Distance is sqrt(0.3^2 + 0.4^2) = 0.5
        float[] vectorA = [0.0f, 0.0f];
        float[] vectorB = [0.3f, 0.4f];

        // Act & Assert with explicit threshold override
        var matchStrict = _sut.Match(vectorA, vectorB, threshold: 0.4);
        var matchRelaxed = _sut.Match(vectorA, vectorB, threshold: 0.6);

        Assert.False(matchStrict.IsMatch);
        Assert.Equal(0.5, matchStrict.Distance, precision: 5);

        Assert.True(matchRelaxed.IsMatch);
        Assert.Equal(0.5, matchRelaxed.Distance, precision: 5);
    }

    [Fact]
    public void Match_WithConfiguredOptions_UsesOptionsThreshold()
    {
        // Arrange
        var customService = new DummyFaceMatchingService(new FaceMatchingOptions { MatchThreshold = 0.4 });
        float[] vectorA = [0.0f, 0.0f];
        float[] vectorB = [0.3f, 0.4f]; // Distance is 0.5 > 0.4

        // Act
        var result = customService.Match(vectorA, vectorB);

        // Assert
        Assert.False(result.IsMatch);
    }

    [Fact]
    public void FindBestMatch_WithClosestCandidateWithinThreshold_ReturnsBestMatch()
    {
        // Arrange
        var employee1 = new NETFace.Attendance.Domain.Entities.Employee("EMP-001", "Alice", false);
        employee1.AddFaceEmbedding([0.1f, 0.1f]);

        var employee2 = new NETFace.Attendance.Domain.Entities.Employee("EMP-002", "Bob", false);
        employee2.AddFaceEmbedding([0.9f, 0.9f]);

        var candidates = new[] { employee1.FaceEmbeddings.First(), employee2.FaceEmbeddings.First() };
        float[] queryVector = [0.12f, 0.11f]; // Closest to employee1

        // Act
        var result = _sut.FindBestMatch(queryVector, candidates);

        // Assert
        Assert.True(result.IsMatch);
        Assert.Equal(employee1.Id, result.MatchedEmployeeId);
        Assert.True(result.Distance < 0.1);
    }

    [Fact]
    public void FindBestMatch_WhenAllCandidatesExceedThreshold_ReturnsNoMatch()
    {
        // Arrange
        var employee1 = new NETFace.Attendance.Domain.Entities.Employee("EMP-001", "Alice", false);
        employee1.AddFaceEmbedding([0.8f, 0.8f]);

        var candidates = new[] { employee1.FaceEmbeddings.First() };
        float[] queryVector = [0.0f, 0.0f]; // Distance ~ 1.13 > 0.6

        // Act
        var result = _sut.FindBestMatch(queryVector, candidates);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Null(result.MatchedEmployeeId);
    }

    [Fact]
    public void FindBestMatch_WithEmptyCandidates_ReturnsNoMatch()
    {
        // Arrange
        float[] queryVector = [0.1f, 0.2f];
        var emptyCandidates = Array.Empty<NETFace.Attendance.Domain.Entities.FaceEmbedding>();

        // Act
        var result = _sut.FindBestMatch(queryVector, emptyCandidates);

        // Assert
        Assert.False(result.IsMatch);
        Assert.Null(result.MatchedEmployeeId);
    }

    [Fact]
    public void CalculateDistance_WithNullVector_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.CalculateDistance(null!, [0.1f]));
        Assert.Throws<ArgumentNullException>(() => _sut.CalculateDistance([0.1f], null!));
    }

    [Fact]
    public void CalculateDistance_WithEmptyVector_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.CalculateDistance([], []));
    }

    [Fact]
    public void CalculateDistance_WithMismatchedDimensions_ThrowsArgumentException()
    {
        float[] vectorA = [0.1f, 0.2f];
        float[] vectorB = [0.1f, 0.2f, 0.3f];

        Assert.Throws<ArgumentException>(() => _sut.CalculateDistance(vectorA, vectorB));
    }

    [Fact]
    public void FindBestMatch_WithNullArguments_ThrowsArgumentNullException()
    {
        var candidates = Array.Empty<NETFace.Attendance.Domain.Entities.FaceEmbedding>();

        Assert.Throws<ArgumentNullException>(() => _sut.FindBestMatch(null!, candidates));
        Assert.Throws<ArgumentNullException>(() => _sut.FindBestMatch([0.1f], null!));
    }
}
