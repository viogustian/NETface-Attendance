using System.Threading;
using System.Threading.Tasks;
using NETFace.Attendance.Application.Interfaces;

namespace NETFace.Attendance.Infrastructure.Services.Recognition;

public class DummyFaceDetectionService : IFaceDetectionService
{
    public Task<FaceDetectionResult> DetectFacesAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        if (imageBytes == null || imageBytes.Length == 0)
        {
            return Task.FromResult(new FaceDetectionResult(FaceDetected: false, FaceCount: 0));
        }

        return Task.FromResult(new FaceDetectionResult(FaceDetected: true, FaceCount: 1));
    }
}
