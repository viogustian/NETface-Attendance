using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        // Legacy accounts before password feature was added will have null PasswordHash
        if (string.IsNullOrEmpty(employee.PasswordHash))
        {
            employee.SetRequiresPasswordChange(true);
            await db.SaveChangesAsync();
        }

        if (!employee.RequiresPasswordChange)
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized(new { message = "Password is required." });
            }

            if (string.IsNullOrEmpty(employee.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, employee.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials or unauthorized." });
            }
        }

        var token = tokenService.GenerateToken(employee);

        return Ok(new AdminLoginResponse(token, employee.EmployeeCode, employee.FullName, employee.RequiresPasswordChange));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long." });
        }

        var employeeCode = User.FindFirst("employeeCode")?.Value;
        if (string.IsNullOrEmpty(employeeCode))
        {
            return Unauthorized(new { message = "Invalid token." });
        }

        var employee = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        if (employee is null)
        {
            return NotFound(new { message = "User not found." });
        }

        employee.SetPassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        employee.SetRequiresPasswordChange(false);
        await db.SaveChangesAsync();

        var token = tokenService.GenerateToken(employee);
        return Ok(new AdminLoginResponse(token, employee.EmployeeCode, employee.FullName, false));
    }
}

public record AdminLoginRequest(string EmployeeCode, string? Password = null);
public record AdminLoginResponse(string Token, string EmployeeCode, string FullName, bool RequiresPasswordChange);
public record ChangePasswordRequest(string NewPassword);
