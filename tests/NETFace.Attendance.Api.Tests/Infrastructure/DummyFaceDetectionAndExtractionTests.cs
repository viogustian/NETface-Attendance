using System;
using System.Threading.Tasks;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class DummyFaceDetectionAndExtractionTests
{
    [Fact]
    public async Task DummyFaceDetection_WithValidBytes_ReturnsDetectedTrue()
    {
        IFaceDetectionService service = new DummyFaceDetectionService();
        byte[] dummyImage = [1, 2, 3, 4];

        var result = await service.DetectFacesAsync(dummyImage);

        Assert.True(result.FaceDetected);
        Assert.Equal(1, result.FaceCount);
    }

    [Fact]
    public async Task DummyFaceDetection_WithEmptyOrNullBytes_ReturnsDetectedFalse()
    {
        IFaceDetectionService service = new DummyFaceDetectionService();

        var resultEmpty = await service.DetectFacesAsync([]);
        var resultNull = await service.DetectFacesAsync(null!);

        Assert.False(resultEmpty.FaceDetected);
        Assert.Equal(0, resultEmpty.FaceCount);
        Assert.False(resultNull.FaceDetected);
        Assert.Equal(0, resultNull.FaceCount);
    }

    [Fact]
    public async Task DummyFaceEmbeddingExtractor_WithValidBytes_Returns128DimensionVector()
    {
        IFaceEmbeddingExtractor extractor = new DummyFaceEmbeddingExtractor();
        byte[] dummyImage = [10, 20, 30];

        var vector = await extractor.ExtractEmbeddingAsync(dummyImage);

        Assert.NotNull(vector);
        Assert.Equal(128, vector.Length);
    }

    [Fact]
    public async Task DummyFaceEmbeddingExtractor_WithSameBytes_ReturnsDeterministicVector()
    {
        IFaceEmbeddingExtractor extractor = new DummyFaceEmbeddingExtractor();
        byte[] dummyImageA = [10, 20, 30];
        byte[] dummyImageB = [10, 20, 30];

        var vectorA = await extractor.ExtractEmbeddingAsync(dummyImageA);
        var vectorB = await extractor.ExtractEmbeddingAsync(dummyImageB);

        Assert.Equal(vectorA, vectorB);
    }

    [Fact]
    public async Task DummyFaceEmbeddingExtractor_WithEmptyBytes_ThrowsArgumentException()
    {
        IFaceEmbeddingExtractor extractor = new DummyFaceEmbeddingExtractor();

        await Assert.ThrowsAsync<ArgumentException>(() => extractor.ExtractEmbeddingAsync([]));
    }
}
