using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Domain.Exceptions;

namespace NETFace.Attendance.Api.Tests.Domain;

public class EmployeeTests
{
    // --- Seam: Employee public interface ---

    [Fact]
    public void Constructor_ShouldInitializeWithActiveStatus()
    {
        var employee = new Employee("EMP001", "Alice Wonderland", isAdmin: false);

        Assert.Equal(EmployeeStatus.Active, employee.Status);
    }

    [Fact]
    public void AddFaceEmbedding_WhenLimitNotReached_ShouldAddEmbedding()
    {
        var employee = new Employee("EMP001", "Alice Wonderland", isAdmin: false);
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        employee.AddFaceEmbedding(vector);

        Assert.Single(employee.FaceEmbeddings);
    }

    [Fact]
    public void AddFaceEmbedding_WhenLimitReached_ShouldThrowException()
    {
        var employee = new Employee("EMP001", "Alice Wonderland", isAdmin: false);

        for (int i = 0; i < 5; i++)
        {
            employee.AddFaceEmbedding(new float[] { (float)i });
        }

        Assert.Throws<MaxFaceEmbeddingsReachedException>(
            () => employee.AddFaceEmbedding(new float[] { 9.9f }));
    }
}
