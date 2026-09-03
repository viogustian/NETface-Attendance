using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=netface_attendance;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.MapControllers();

app.Run();

// Expose Program as a partial class for WebApplicationFactory in tests
public partial class Program { }
