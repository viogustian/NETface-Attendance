using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Infrastructure.Services.Recognition;

namespace NETFace.Attendance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRecognitionServices(this IServiceCollection services, IConfiguration? configuration = null)
    {
        var options = new FaceMatchingOptions();
        var thresholdValue = configuration?["FaceMatching:MatchThreshold"];
        if (double.TryParse(thresholdValue, System.Globalization.CultureInfo.InvariantCulture, out double parsedThreshold))
        {
            options.MatchThreshold = parsedThreshold;
        }

        services.AddSingleton(options);
        services.AddScoped<IFaceDetectionService, DummyFaceDetectionService>();
        services.AddScoped<IFaceEmbeddingExtractor, DummyFaceEmbeddingExtractor>();
        services.AddScoped<IFaceMatchingService, DummyFaceMatchingService>();

        return services;
    }
}
