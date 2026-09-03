using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.MapControllers();

app.Run();

// Expose Program as a partial class for WebApplicationFactory in tests
public partial class Program { }
