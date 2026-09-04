using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NETFace.Attendance.Infrastructure.Persistence;
using Xunit;

namespace NETFace.Attendance.Api.Tests.Api;

public class EmployeesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _baseFactory;

    public EmployeesControllerTests(WebApplicationFactory<Program> factory)
    {
        _baseFactory = factory;
    }

    /// <summary>
    /// Creates a new client backed by an isolated in-memory database.
    /// Each call produces a fresh database to prevent test data leakage.
    /// </summary>
    private HttpClient CreateIsolatedClient()
    {
        var dbName = $"InMemoryDb_{Guid.NewGuid()}";

        var factory = _baseFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });

        return factory.CreateClient();
    }

    [Fact]
    public async Task CreateEmployee_ReturnsCreatedEmployee()
    {
        // Arrange
        var client = CreateIsolatedClient();
        var request = new
        {
            EmployeeCode = "TEST-001",
            ProfileDetails = "Test Employee",
            AdminFlag = false
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/employees", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("TEST-001", responseString);
    }

    [Fact]
    public async Task ListEmployees_ReturnsOkWithEmployees()
    {
        // Arrange
        var client = CreateIsolatedClient();

        var emp1 = new { EmployeeCode = "LIST-001", ProfileDetails = "Alice", AdminFlag = false };
        var emp2 = new { EmployeeCode = "LIST-002", ProfileDetails = "Bob",   AdminFlag = true  };

        await client.PostAsJsonAsync("/api/employees", emp1);
        await client.PostAsJsonAsync("/api/employees", emp2);

        // Act
        var response = await client.GetAsync("/api/employees");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("LIST-001", responseString);
        Assert.Contains("LIST-002", responseString);
    }
}
