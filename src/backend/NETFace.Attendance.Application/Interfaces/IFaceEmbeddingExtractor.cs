using System.Threading;
using System.Threading.Tasks;

namespace NETFace.Attendance.Application.Interfaces;

public interface IFaceEmbeddingExtractor
{
    Task<float[]> ExtractEmbeddingAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}
