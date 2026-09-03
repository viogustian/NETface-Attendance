using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NETFace.Attendance.Api.Authentication;
using NETFace.Attendance.Api.Services;
using NETFace.Attendance.Infrastructure;
using NETFace.Attendance.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Register Recognition Services (dummy services for ML/AI abstraction)
builder.Services.AddRecognitionServices(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configure Authentication & Authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Combined";
    options.DefaultAuthenticateScheme = "Combined";
    options.DefaultChallengeScheme = "Combined";
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme)
.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
    ApiKeyAuthenticationOptions.DefaultScheme, _ => { })
.AddPolicyScheme("Combined", "JWT or API Key", options =>
{
    options.ForwardDefaultSelector = context =>
    {
        if (context.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.ApiKeyHeaderName) ||
            context.Request.Headers.ContainsKey(ApiKeyAuthenticationHandler.DeviceTokenHeaderName))
        {
            return ApiKeyAuthenticationOptions.DefaultScheme;
        }
        return JwtBearerDefaults.AuthenticationScheme;
    };
});

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, config) =>
    {
        var key = config["Jwt:Key"]
            ?? "NETFace-Super-Secret-Key-Must-Be-At-Least-32-Bytes-Long-123456";
        var issuer = config["Jwt:Issuer"] ?? "NETFace.Attendance.Api";
        var audience = config["Jwt:Audience"] ?? "NETFace.Attendance.Clients";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });

builder.Services.AddOptions<ApiKeyAuthenticationOptions>(ApiKeyAuthenticationOptions.DefaultScheme)
    .Configure<IConfiguration>((options, config) =>
    {
        options.ApiKey = config["DeviceAuth:ApiKey"] ?? "netface-terminal-default-api-key";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expose Program as a partial class for WebApplicationFactory in tests
public partial class Program { }
