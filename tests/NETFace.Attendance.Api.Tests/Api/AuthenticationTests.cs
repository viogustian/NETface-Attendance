using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Infrastructure.Persistence;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Api;

public class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    private const string TestJwtKey = "NETFace-Super-Secret-Test-Key-With-At-Least-32-Bytes-Long!";
    private const string TestJwtIssuer = "NETFace.Attendance.Api";
    private const string TestJwtAudience = "NETFace.Attendance.Clients";
    private const string ValidApiKey = "netface-terminal-default-api-key";

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private (HttpClient Client, IServiceProvider Services) CreateClientWithIsolatedDb(
        Action<IServiceCollection>? configureServices = null)
    {
        var dbName = Guid.NewGuid().ToString();
        var customFactory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var inMemorySettings = new Dictionary<string, string?>
                {
                    { "Jwt:Key", TestJwtKey },
                    { "Jwt:Issuer", TestJwtIssuer },
                    { "Jwt:Audience", TestJwtAudience },
                    { "Jwt:ExpireMinutes", "60" },
                    { "DeviceAuth:ApiKey", ValidApiKey }
                };
                config.AddInMemoryCollection(inMemorySettings);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                configureServices?.Invoke(services);
            });
        });

        return (customFactory.CreateClient(), customFactory.Services);
    }

    private static string GenerateTestJwt(string employeeId, string employeeCode, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, employeeId),
            new Claim(ClaimTypes.NameIdentifier, employeeId),
            new Claim("employeeCode", employeeCode),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: TestJwtIssuer,
            audience: TestJwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Theory]
    [InlineData("/api/employees", "GET")]
    [InlineData("/api/employees", "POST")]
    [InlineData("/api/attendance-sessions", "POST")]
    [InlineData("/api/recognition/attempt", "POST")]
    public async Task UnauthenticatedRequests_ToRestrictedRoutes_ReturnUnauthorized401(string route, string method)
    {
        var (client, _) = CreateClientWithIsolatedDb();

        var request = new HttpRequestMessage(new HttpMethod(method), route);
        if (method == "POST")
        {
            request.Content = JsonContent.Create(new { });
        }

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminLogin_WithNonExistentEmployee_ReturnsUnauthorized401()
    {
        var (client, _) = CreateClientWithIsolatedDb();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            employeeCode = "NONEXISTENT"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminLogin_WithNonAdminEmployee_ReturnsUnauthorized401()
    {
        var (client, services) = CreateClientWithIsolatedDb();

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var nonAdmin = new Employee("EMP999", "Regular User", isAdmin: false);
            db.Employees.Add(nonAdmin);
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            employeeCode = "EMP999"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task AdminLogin_WithValidAdminEmployee_ReturnsOkWithJwtToken()
    {
        var (client, services) = CreateClientWithIsolatedDb();

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = new Employee("ADM001", "Admin Boss", isAdmin: true);
            db.Employees.Add(admin);
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            employeeCode = "ADM001"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("ADM001", body.EmployeeCode);
        Assert.Equal("Admin Boss", body.FullName);
        Assert.True(body.RequiresPasswordChange);
    }

    [Fact]
    public async Task AdminJwt_CanAccessAdminEndpoints_ReturnsSuccess()
    {
        var (client, services) = CreateClientWithIsolatedDb();

        using (var scope = services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var admin = new Employee("ADM001", "Admin Boss", isAdmin: true);
            db.Employees.Add(admin);
            await db.SaveChangesAsync();
        }

        var adminToken = GenerateTestJwt(Guid.NewGuid().ToString(), "ADM001", "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync("/api/employees");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminJwt_CannotAccessDeviceRoutes_ReturnsForbidden403()
    {
        var (client, _) = CreateClientWithIsolatedDb();

        var adminToken = GenerateTestJwt(Guid.NewGuid().ToString(), "ADM001", "Admin");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/recognition/attempt");
        request.Content = new ByteArrayContent(new byte[] { 1, 2, 3 });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeviceToken_CanAccessDeviceEndpoint_ReturnsOkOrBadRequest_NotUnauthorizedOrForbidden()
    {
        var (client, _) = CreateClientWithIsolatedDb();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/recognition/attempt");
        request.Headers.Add("X-Api-Key", ValidApiKey);
        request.Content = new ByteArrayContent(new byte[] { 1, 2, 3 });

        var response = await client.SendAsync(request);

        // Should NOT be 401 or 403. It will process the image (OK or BadRequest depending on payload).
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeviceToken_CannotAccessAdminRoutes_ReturnsForbidden403()
    {
        var (client, _) = CreateClientWithIsolatedDb();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/employees");
        request.Headers.Add("X-Api-Key", ValidApiKey);

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InvalidDeviceToken_ToDeviceEndpoint_ReturnsUnauthorized401()
    {
        var (client, _) = CreateClientWithIsolatedDb();

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/recognition/attempt");
        request.Headers.Add("X-Api-Key", "invalid-bad-key");
        request.Content = new ByteArrayContent(new byte[] { 1, 2, 3 });

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private record LoginResponse(string Token, string EmployeeCode, string FullName, bool RequiresPasswordChange);
}
