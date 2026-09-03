using Microsoft.Extensions.Options;
using NETFace.Attendance.Infrastructure.Services.Recognition;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Infrastructure;

public class OnnxSessionManagerTests
{
    [Fact]
    public void Constructor_WhenModelFilesAreMissing_ThrowsFileNotFoundException()
    {
        // Arrange
        var options = Options.Create(new OnnxModelOptions
        {
            YuNetModelPath = "fake_path_yunet.onnx",
            SFaceModelPath = "fake_path_sface.onnx"
        });

        // Act & Assert
        var ex = Assert.Throws<FileNotFoundException>(() => new OnnxSessionManager(options));
        Assert.Contains("YuNet ONNX Model not found", ex.Message);
    }

    [Fact]
    public void Constructor_WhenYuNetExistsButSFaceMissing_ThrowsFileNotFoundException()
    {
        // Arrange
        string tempYuNet = Path.GetTempFileName();
        try
        {
            var options = Options.Create(new OnnxModelOptions
            {
                YuNetModelPath = tempYuNet,
                SFaceModelPath = "fake_path_sface.onnx"
            });

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => new OnnxSessionManager(options));
            Assert.Contains("SFace ONNX Model not found", ex.Message);
        }
        finally
        {
            if (File.Exists(tempYuNet)) File.Delete(tempYuNet);
        }
    }
}
