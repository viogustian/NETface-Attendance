using System;
using Xunit;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Domain.Exceptions;

namespace NETFace.Attendance.Api.Tests.Domain;

public class EmployeeTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithActiveStatus()
    {
        var employee = new Employee("EMP-001", "Alice Wonderland", isAdmin: false);

        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.Equal("EMP-001", employee.EmployeeCode);
        Assert.Equal("Alice Wonderland", employee.FullName);
        Assert.False(employee.IsAdmin);
        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Empty(employee.FaceEmbeddings);
    }

    [Fact]
    public void AddFaceEmbedding_WhenLimitNotReached_ShouldAddEmbedding()
    {
        var employee = new Employee("EMP-001", "Alice Wonderland", isAdmin: false);
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        employee.AddFaceEmbedding(vector);
        Assert.Single(employee.FaceEmbeddings);
    }

    [Fact]
    public void AddFaceEmbedding_WhenLimitReached_ShouldThrowException()
    {
        var employee = new Employee("EMP-001", "Alice Wonderland", isAdmin: false);

        for (int i = 0; i < 5; i++)
        {
            employee.AddFaceEmbedding(new float[] { (float)i });
        }

        var exception = Assert.Throws<MaxFaceEmbeddingsReachedException>(
            () => employee.AddFaceEmbedding(new float[] { 9.9f }));

        Assert.Equal("An employee can have a maximum of 5 face embeddings.", exception.Message);
    }
}
