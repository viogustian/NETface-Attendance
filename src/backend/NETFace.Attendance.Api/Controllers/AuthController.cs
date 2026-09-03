using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Api.Services;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeCode))
        {
            return Unauthorized(new { message = "Employee code is required." });
        }

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == request.EmployeeCode);

        if (employee is null || !employee.IsAdmin || employee.Status != EmployeeStatus.Active)
        {
            return Unauthorized(new { message = "Invalid credentials or unauthorized." });
        }

        var token = tokenService.GenerateToken(employee);

        return Ok(new AdminLoginResponse(token, employee.EmployeeCode, employee.FullName));
    }
}

public record AdminLoginRequest(string EmployeeCode, string? Password = null);
public record AdminLoginResponse(string Token, string EmployeeCode, string FullName);
