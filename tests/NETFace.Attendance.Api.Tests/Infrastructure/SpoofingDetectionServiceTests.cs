using NETFace.Attendance.Infrastructure.Services.Recognition;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class SpoofingDetectionServiceTests
{
    [Fact]
    public void RecordUnknownFace_IncrementsFailureCount_AndDetectsSpoofingAtThreshold()
    {
        // Arrange
        var sut = new SpoofingDetectionService(threshold: 3);
        string key = "terminal-1";

        // Act & Assert
        Assert.False(sut.IsSpoofingSuspected(key));

        int count1 = sut.RecordUnknownFace(key);
        Assert.Equal(1, count1);
        Assert.False(sut.IsSpoofingSuspected(key));

        int count2 = sut.RecordUnknownFace(key);
        Assert.Equal(2, count2);
        Assert.False(sut.IsSpoofingSuspected(key));

        int count3 = sut.RecordUnknownFace(key);
        Assert.Equal(3, count3);
        Assert.True(sut.IsSpoofingSuspected(key));
    }

    [Fact]
    public void RecordSuccess_ResetsFailureCount()
    {
        // Arrange
        var sut = new SpoofingDetectionService(threshold: 3);
        string key = "terminal-1";

        sut.RecordUnknownFace(key);
        sut.RecordUnknownFace(key);
        Assert.False(sut.IsSpoofingSuspected(key));

        // Act
        sut.RecordSuccess(key);

        // Assert
        Assert.False(sut.IsSpoofingSuspected(key));
        int newCount = sut.RecordUnknownFace(key);
        Assert.Equal(1, newCount);
    }

    [Fact]
    public void Reset_ClearsFailureCount()
    {
        // Arrange
        var sut = new SpoofingDetectionService(threshold: 2);
        string key = "terminal-1";

        sut.RecordUnknownFace(key);
        sut.RecordUnknownFace(key);
        Assert.True(sut.IsSpoofingSuspected(key));

        // Act
        sut.Reset(key);

        // Assert
        Assert.False(sut.IsSpoofingSuspected(key));
    }
}
