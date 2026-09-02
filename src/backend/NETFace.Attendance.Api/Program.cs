using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// We register DbContext here.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=NETFace_Attendance;Username=postgres;Password=postgres"));

var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }
