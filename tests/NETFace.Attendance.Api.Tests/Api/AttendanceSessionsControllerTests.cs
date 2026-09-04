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

namespace NETFace.Attendance.Api.Tests.Api;

public class AttendanceSessionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AttendanceSessionsControllerTests(WebApplicationFactory<Program> factory)
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

    private HttpClient CreateClientWithIsolatedDb()
    {
        var dbName = Guid.NewGuid().ToString();
        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));
            });
        }).CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateAdminToken());
        return client;
    }

    private static object BuildCreateRequest(int employeeCount = 2) => new
    {
        departmentName = "Engineering",
        date = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
        employees = Enumerable.Range(1, employeeCount).Select(i => new
        {
            employeeId = Guid.NewGuid().ToString(),
            employeeCode = $"EMP{i:D3}",
            employeeName = $"Employee {i}",
        }).ToList(),
    };

    [Fact]
    public async Task CreateSession_ReturnsCreatedSession_WithAbsentEntries()
    {
        var client = CreateClientWithIsolatedDb();

        var response = await client.PostAsJsonAsync("/api/attendance-sessions", BuildCreateRequest(3));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(body);
        Assert.Equal("Engineering", body.DepartmentName);
        Assert.Equal(3, body!.Entries.Count);
    }

    [Fact]
    public async Task FinalizeSession_ReturnsOkWithFinalizedStatus()
    {
        var client = CreateClientWithIsolatedDb();

        var createResponse = await client.PostAsJsonAsync("/api/attendance-sessions", BuildCreateRequest(1));
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var finalizeResponse = await client.PutAsync($"/api/attendance-sessions/{created!.Id}/finalize", null);

        Assert.Equal(HttpStatusCode.OK, finalizeResponse.StatusCode);

        var body = await finalizeResponse.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(body);
        Assert.Equal("Finalized", body.Status);
    }

    [Fact]
    public async Task CancelSession_ReturnsOkWithCancelledStatus_AndEntriesRetained()
    {
        var client = CreateClientWithIsolatedDb();

        var createResponse = await client.PostAsJsonAsync("/api/attendance-sessions", BuildCreateRequest(2));
        var created = await createResponse.Content.ReadFromJsonAsync<SessionResponse>();

        var cancelResponse = await client.PutAsync($"/api/attendance-sessions/{created!.Id}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var body = await cancelResponse.Content.ReadFromJsonAsync<SessionResponse>();
        Assert.NotNull(body);
        Assert.Equal("Cancelled", body.Status);
        Assert.Equal(2, body!.Entries.Count);
    }

    private record SessionResponse(Guid Id, string DepartmentName, string Date, string Status, List<System.Text.Json.JsonElement> Entries);
}
