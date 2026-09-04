using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class OnnxModelOptions
{
    public const string SectionName = "OnnxModels";
    public string YuNetModelPath { get; set; } = string.Empty;
    public string SFaceModelPath { get; set; } = string.Empty;
    public int? MaxConcurrentInference { get; set; }
}

public interface IOnnxSessionManager : IDisposable
{
    InferenceSession YuNetSession { get; }
    InferenceSession SFaceSession { get; }
    SemaphoreSlim InferenceThrottle { get; }
}

public class OnnxSessionManager : IOnnxSessionManager
{
    public InferenceSession YuNetSession { get; }
    public InferenceSession SFaceSession { get; }
    public SemaphoreSlim InferenceThrottle { get; }

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

        int maxConcurrency = config.MaxConcurrentInference.HasValue && config.MaxConcurrentInference.Value > 0
            ? config.MaxConcurrentInference.Value
            : Math.Max(1, Environment.ProcessorCount);

        InferenceThrottle = new SemaphoreSlim(maxConcurrency, maxConcurrency);
    }

    public void Dispose()
    {
        YuNetSession?.Dispose();
        SFaceSession?.Dispose();
        InferenceThrottle?.Dispose();
    }
}
