using System.Threading;
using System.Threading.Tasks;

namespace NETFace.Attendance.Application.Interfaces;

public interface IFaceDetectionService
{
    Task<FaceDetectionResult> DetectFacesAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}
