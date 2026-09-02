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
    private readonly WebApplicationFactory<Program> _factory;

    public EmployeesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
    }

    [Fact]
    public async Task CreateEmployee_ReturnsCreatedEmployee()
    {
        // Arrange
        var client = _factory.CreateClient();
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
}
