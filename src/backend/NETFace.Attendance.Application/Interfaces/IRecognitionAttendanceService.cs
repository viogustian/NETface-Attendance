using System;
using System.Threading;
using System.Threading.Tasks;
using NETFace.Attendance.Application.DTOs;

namespace NETFace.Attendance.Application.Interfaces;

public interface IRecognitionAttendanceService
{
    Task<RecognitionAttemptResult> AttemptRecognitionAsync(
        byte[] imageBytes,
        Guid? sessionId = null,
        CancellationToken cancellationToken = default);
}
