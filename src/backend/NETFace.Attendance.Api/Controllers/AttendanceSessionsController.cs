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

        var session = AttendanceSession.Create(request.DepartmentName, request.Date, roster);

        db.AttendanceSessions.Add(session);
        await db.SaveChangesAsync();

        var response = ToResponse(session);
        return CreatedAtAction(nameof(GetById), new { id = session.Id }, response);
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

    private static AttendanceSessionResponse ToResponse(AttendanceSession s) =>
        new(s.Id, s.DepartmentName, s.Date.ToString("yyyy-MM-dd"), s.Status.ToString(), s.Entries.Select(e => new AttendanceEntryResponse(e.Id, e.EmployeeId, e.EmployeeCode, e.EmployeeName, e.Status.ToString())).ToList());
}
