using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NETFace.Attendance.Infrastructure.Persistence;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Api;

public class EmployeesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EmployeesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static string GenerateAdminToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("NETFace-Super-Secret-Key-Must-Be-At-Least-32-Bytes-Long-123456"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, "Admin") };
        var token = new JwtSecurityToken(
            issuer: "NETFace.Attendance.Api",
            audience: "NETFace.Attendance.Clients",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a new client backed by an isolated in-memory database.
    /// Each call produces a fresh database to prevent test data leakage.
    /// </summary>
    private HttpClient CreateClientWithIsolatedDb()
    {
        var dbName = $"InMemoryDb_{Guid.NewGuid()}";
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                // Use unique in-memory DB per test
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAdminToken());
        return client;
    }

    [Fact]
    public async Task CreateEmployee_ReturnsCreatedEmployee()
    {
        var client = CreateClientWithIsolatedDb();

        var response = await client.PostAsJsonAsync("/api/employees", new
        {
            employeeCode = "EMP001",
            fullName = "Alice Wonderland",
            isAdmin = false,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<EmployeeResponse>();
        Assert.NotNull(body);
        Assert.Equal("EMP001", body.EmployeeCode);
        Assert.Equal("Alice Wonderland", body.FullName);
    }

    [Fact]
    public async Task ListEmployees_ReturnsOkWithEmployees()
    {
        var client = CreateClientWithIsolatedDb();

        // Seed one employee first
        await client.PostAsJsonAsync("/api/employees", new
        {
            employeeCode = "EMP002",
            fullName = "Bob Builder",
            isAdmin = false,
        });

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var employees = await response.Content.ReadFromJsonAsync<List<EmployeeResponse>>();
        Assert.NotNull(employees);
        Assert.Single(employees);
        Assert.Equal("EMP002", employees[0].EmployeeCode);
    }

    // DTO mirror for assertion (avoids referencing Api DTOs directly)
    private record EmployeeResponse(Guid Id, string EmployeeCode, string FullName, bool IsAdmin, string Status);
}
