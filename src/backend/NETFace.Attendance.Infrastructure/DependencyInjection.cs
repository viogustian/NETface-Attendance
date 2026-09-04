using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Infrastructure.Services.Recognition;

namespace NETFace.Attendance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRecognitionServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        if (configuration != null)
        {
            services.Configure<FaceMatchingOptions>(config =>
            {
                var thresholdValue = configuration?["FaceMatching:MatchThreshold"];
                if (double.TryParse(thresholdValue, System.Globalization.CultureInfo.InvariantCulture, out double parsedThreshold))
                {
                    config.MatchThreshold = parsedThreshold;
                }
            });
        }

        services.AddScoped<IFaceDetectionService, YuNetFaceDetectionService>();
        services.AddScoped<IFaceEmbeddingExtractor, SFaceEmbeddingExtractor>();
        services.AddScoped<IFaceMatchingService, YuNetFaceMatchingService>();
        services.AddScoped<IRecognitionAttendanceService, RecognitionAttendanceService>();
        services.AddSingleton<ISpoofingDetectionService, SpoofingDetectionService>();

        if (configuration != null)
        {
            services.Configure<OnnxModelOptions>(opts => 
            {
                var section = configuration.GetSection(OnnxModelOptions.SectionName);
                opts.YuNetModelPath = section["YuNetModelPath"] ?? string.Empty;
                opts.SFaceModelPath = section["SFaceModelPath"] ?? string.Empty;
            });
            services.AddSingleton<IOnnxSessionManager, OnnxSessionManager>();
        }

        return services;
    }
}
