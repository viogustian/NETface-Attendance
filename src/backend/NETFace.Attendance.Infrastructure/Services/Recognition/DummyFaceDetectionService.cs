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
            return Task.FromResult(new FaceDetectionResult(faceDetected: false, faceCount: 0));
        }

        return Task.FromResult(new FaceDetectionResult(faceDetected: true, faceCount: 1));
    }

    public Task<FaceDetectionResult> DetectFacesAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new FaceDetectionResult(faceDetected: true, faceCount: 1));
    }
}
