using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class OnnxModelOptions
{
    public const string SectionName = "OnnxModels";
    public string YuNetModelPath { get; set; } = string.Empty;
    public string SFaceModelPath { get; set; } = string.Empty;
}

public interface IOnnxSessionManager : IDisposable
{
    InferenceSession YuNetSession { get; }
    InferenceSession SFaceSession { get; }
}

public class OnnxSessionManager : IOnnxSessionManager
{
    public InferenceSession YuNetSession { get; }
    public InferenceSession SFaceSession { get; }

    public OnnxSessionManager(IOptions<OnnxModelOptions> options)
    {
        var config = options.Value;
        
        if (string.IsNullOrWhiteSpace(config.YuNetModelPath) || !File.Exists(config.YuNetModelPath))
            throw new FileNotFoundException($"YuNet ONNX Model not found at path: {config.YuNetModelPath}");
        
        if (string.IsNullOrWhiteSpace(config.SFaceModelPath) || !File.Exists(config.SFaceModelPath))
            throw new FileNotFoundException($"SFace ONNX Model not found at path: {config.SFaceModelPath}");

        var sessionOptions = new SessionOptions();
        // CPU default
        
        YuNetSession = new InferenceSession(config.YuNetModelPath, sessionOptions);
        SFaceSession = new InferenceSession(config.SFaceModelPath, sessionOptions);
    }

    public void Dispose()
    {
        YuNetSession?.Dispose();
        SFaceSession?.Dispose();
    }
}
