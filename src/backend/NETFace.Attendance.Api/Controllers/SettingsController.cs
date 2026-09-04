using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/settings")]
public class SettingsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await db.SystemSettings.ToListAsync();
        var dictionary = settings.ToDictionary(s => s.Key, s => s.Value);
        
        // Ensure default exists
        if (!dictionary.ContainsKey("ClockOutStartTime"))
        {
            dictionary["ClockOutStartTime"] = "12:00:00";
        }
        
        return Ok(dictionary);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> newSettings)
    {
        foreach (var (key, value) in newSettings)
        {
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                db.SystemSettings.Add(new SystemSetting(key, value));
            }
            else
            {
                setting.UpdateValue(value);
            }
        }
        
        await db.SaveChangesAsync();
        return Ok(new { message = "Settings updated successfully." });
    }
}
