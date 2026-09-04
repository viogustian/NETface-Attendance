using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Api.Controllers.AttendanceSessions;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Exceptions;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/attendance-sessions")]
public class AttendanceSessionsController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAttendanceSessionRequest request)
    {
        var roster = request.Employees
            .Select(e => (e.EmployeeId, e.EmployeeCode, e.EmployeeName))
            .ToList();

        var date = DateOnly.FromDateTime(DateTime.Now.Date);
        var session = AttendanceSession.Create(request.DepartmentName, date, roster);

        db.AttendanceSessions.Add(session);
        await db.SaveChangesAsync();

        var response = ToResponse(session);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sessions = await db.AttendanceSessions
            .Include(s => s.Entries)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return Ok(sessions.Select(ToResponse));
    }

    [HttpGet("{id:guid}", Name = nameof(GetById))]
    public async Task<IActionResult> GetById(Guid id)
    {
        var session = await db.AttendanceSessions
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null)
            return NotFound();

        return Ok(ToResponse(session));
    }
    
    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> ExportCsv(Guid id)
    {
        var session = await db.AttendanceSessions
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null)
            return NotFound();

        var sb = new StringBuilder();
        sb.AppendLine("Employee Code,Name,Status,Clock In,Clock Out,Total Hours");
        
        foreach (var entry in session.Entries.OrderBy(e => e.EmployeeName))
        {
            var clockIn = entry.ClockInTime?.ToLocalTime().ToString("HH:mm:ss") ?? "-";
            var clockOut = entry.ClockOutTime?.ToLocalTime().ToString("HH:mm:ss") ?? "-";
            var totalHours = entry.TotalWorkHours?.ToString("F2") ?? "-";
            sb.AppendLine($"{entry.EmployeeCode},{entry.EmployeeName},{entry.Status},{clockIn},{clockOut},{totalHours}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var filename = $"Attendance_{session.DepartmentName}_{session.Date:yyyy-MM-dd}.csv";
        return File(bytes, "text/csv", filename);
    }

    [HttpPut("{id:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid id)
    {
        var session = await db.AttendanceSessions
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null)
            return NotFound();

        try
        {
            session.FinalizeSession();
        }
        catch (AttendanceSessionAlreadyFinalizedException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        await db.SaveChangesAsync();
        return Ok(ToResponse(session));
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var session = await db.AttendanceSessions
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null)
            return NotFound();

        try
        {
            session.Cancel();
        }
        catch (AttendanceSessionAlreadyFinalizedException ex)
        {
            return Conflict(new { message = ex.Message });
        }

        await db.SaveChangesAsync();
        return Ok(ToResponse(session));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var session = await db.AttendanceSessions
            .Include(s => s.Entries)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session is null)
            return NotFound(new { message = "Session not found." });

        db.AttendanceSessions.Remove(session);
        await db.SaveChangesAsync();

        return Ok(new { message = "Session deleted successfully." });
    }

    private static AttendanceSessionResponse ToResponse(AttendanceSession s) =>
        new(s.Id, s.DepartmentName, s.Date.ToString("yyyy-MM-dd"), s.Status.ToString(), s.Entries.Select(e => new AttendanceEntryResponse(e.Id, e.EmployeeId, e.EmployeeCode, e.EmployeeName, e.Status.ToString(), e.ClockInTime, e.ClockOutTime, e.TotalWorkHours)).ToList());
}
