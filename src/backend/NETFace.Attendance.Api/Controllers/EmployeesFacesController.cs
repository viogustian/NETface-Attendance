using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Api.Controllers.Employees;
using NETFace.Attendance.Application.Interfaces;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/employees/{id:guid}/faces")]
public class EmployeesFacesController(
    AppDbContext db,
    IFaceDetectionService faceDetectionService,
    IFaceEmbeddingExtractor faceEmbeddingExtractor) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> EnrollFaces(Guid id, List<IFormFile> files, CancellationToken cancellationToken)
    {
        var employee = await db.Employees
            .Include(e => e.FaceEmbeddings)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
            return NotFound(new { message = "Employee not found." });

        if (files == null || files.Count == 0)
            return BadRequest(new { message = "No files uploaded." });

        if (files.Count > 3)
            return BadRequest(new { message = "Maksimal 3 foto dapat diunggah sekaligus." });

        var performedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                          User.Identity?.Name ?? 
                          "System";

        if (employee.FaceEmbeddings.Count + files.Count > 5)
        {
            var errLog = EnrollmentLog.CreateFailed(id, EnrollmentAction.FACE_ENROLLMENT, files.Count, performedBy, "Kuota penuh");
            db.EnrollmentLogs.Add(errLog);
            await db.SaveChangesAsync(cancellationToken);
            return BadRequest(new { message = "Employee sudah memiliki 5 face embedding. Hapus/reset embedding lama untuk mendaftarkan wajah baru." });
        }

        using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var file in files)
            {
                if (file.Length > 5 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Ukuran foto terlalu besar. Maksimum 5 MB.");
                }

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    throw new InvalidOperationException("File harus berupa JPG atau PNG.");
                }

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms, cancellationToken);
                var imageBytes = ms.ToArray();

                var detectionResult = await faceDetectionService.DetectFacesAsync(imageBytes, cancellationToken);
                
                if (!detectionResult.Success)
                {
                    if (detectionResult.ErrorCode == "multi_face_detected")
                        throw new InvalidOperationException("Terdeteksi lebih dari satu wajah. Pastikan hanya satu orang berada di depan kamera.");
                    if (detectionResult.ErrorCode == "no_face_detected")
                        throw new InvalidOperationException("Wajah tidak ditemukan. Pastikan wajah terlihat jelas.");
                    if (detectionResult.ErrorCode == "low_confidence")
                        throw new InvalidOperationException("Foto terlalu buram. Ambil foto dengan wajah yang lebih jelas.");
                    
                    throw new InvalidOperationException(detectionResult.ErrorMessage ?? "Validasi wajah gagal.");
                }

                try
                {
                    var embedding = await faceEmbeddingExtractor.ExtractEmbeddingAsync(imageBytes, cancellationToken);
                    employee.AddFaceEmbedding(embedding);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Foto berhasil dikirim tetapi wajah gagal diproses. Detail: {ex.Message}", ex);
                }
            }

            var log = EnrollmentLog.CreateSuccess(id, EnrollmentAction.FACE_ENROLLMENT, files.Count, files.Count, performedBy);
            db.EnrollmentLogs.Add(log);

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var remainingSlots = 5 - employee.FaceEmbeddings.Count;
            return Ok(new FaceEnrollmentResponse(true, employee.Id, files.Count, remainingSlots));
        }
        catch (InvalidOperationException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            
            var errLog = EnrollmentLog.CreateFailed(id, EnrollmentAction.FACE_ENROLLMENT, files.Count, performedBy, ex.Message);
            db.EnrollmentLogs.Add(errLog);
            await db.SaveChangesAsync(cancellationToken);
            
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            db.ChangeTracker.Clear();
            
            var errLog = EnrollmentLog.CreateFailed(id, EnrollmentAction.FACE_ENROLLMENT, files.Count, performedBy, "Internal Server Error");
            db.EnrollmentLogs.Add(errLog);
            await db.SaveChangesAsync(cancellationToken);
            
            return StatusCode(500, new { message = "Enrollment gagal karena server tidak dapat memproses permintaan. Silakan coba lagi.", error = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> ClearFaces(Guid id, CancellationToken cancellationToken)
    {
        var employee = await db.Employees
            .Include(e => e.FaceEmbeddings)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (employee is null)
            return NotFound(new { message = "Employee not found." });

        var performedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                          User.Identity?.Name ?? 
                          "System";
        
        int clearedCount = employee.FaceEmbeddings.Count;
        
        employee.ClearFaceEmbeddings();
        
        var log = EnrollmentLog.CreateSuccess(id, EnrollmentAction.FACE_CLEAR, 0, clearedCount, performedBy);
        db.EnrollmentLogs.Add(log);

        await db.SaveChangesAsync(cancellationToken);

        return Ok(new FaceClearResponse(true, employee.Id, clearedCount, 5));
    }
}
