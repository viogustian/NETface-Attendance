using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NETFace.Attendance.Application.Interfaces;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Route("api/recognition")]
public class RecognitionController(IRecognitionAttendanceService recognitionService) : ControllerBase
{
    [HttpPost("attempt")]
    public async Task<IActionResult> Attempt([FromQuery] Guid? sessionId, CancellationToken cancellationToken)
    {
        byte[]? imageBytes = null;
        Guid? targetSessionId = sessionId;

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            var file = form.Files.GetFile("image") ?? form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
            if (file is not null)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                imageBytes = ms.ToArray();
            }

            if (form.TryGetValue("sessionId", out var formSessionId) && Guid.TryParse(formSessionId, out var parsedSessionId))
            {
                targetSessionId = parsedSessionId;
            }
        }
        else if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            var jsonRequest = await Request.ReadFromJsonAsync<RecognitionAttemptJsonRequest>(cancellationToken: cancellationToken);
            if (jsonRequest is not null)
            {
                if (!string.IsNullOrWhiteSpace(jsonRequest.Image))
                {
                    try
                    {
                        imageBytes = Convert.FromBase64String(jsonRequest.Image);
                    }
                    catch (FormatException)
                    {
                        return BadRequest(new RecognitionAttemptResponse(
                            Success: false,
                            Message: "Invalid base64 image data."));
                    }
                }

                if (jsonRequest.SessionId.HasValue)
                {
                    targetSessionId = jsonRequest.SessionId;
                }
            }
        }
        else
        {
            // Raw binary stream in body
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms, cancellationToken);
            imageBytes = ms.ToArray();
        }

        if (imageBytes is null || imageBytes.Length == 0)
        {
            var emptyResult = await recognitionService.AttemptRecognitionAsync([], targetSessionId, cancellationToken);
            return BadRequest(new RecognitionAttemptResponse(
                Success: false,
                Message: emptyResult.Message,
                RecognitionLogId: emptyResult.RecognitionLogId));
        }

        var result = await recognitionService.AttemptRecognitionAsync(imageBytes, targetSessionId, cancellationToken);

        var response = new RecognitionAttemptResponse(
            Success: result.Success,
            Message: result.Message,
            EmployeeId: result.EmployeeId,
            EmployeeCode: result.EmployeeCode,
            EmployeeName: result.EmployeeName,
            MarkedAt: result.MarkedAt,
            Confidence: result.Confidence,
            RecognitionLogId: result.RecognitionLogId);

        return Ok(response);
    }
}

public record RecognitionAttemptJsonRequest(string? Image, Guid? SessionId);

public record RecognitionAttemptResponse(
    bool Success,
    string Message,
    Guid? EmployeeId = null,
    string? EmployeeCode = null,
    string? EmployeeName = null,
    DateTimeOffset? MarkedAt = null,
    double Confidence = 0.0,
    Guid? RecognitionLogId = null);
