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
        // Arrange & Act
        var employee = new Employee("EMP-001", "John Doe", false);

        // Assert
        Assert.Equal(EmployeeStatus.Active, employee.Status);
        Assert.Equal("EMP-001", employee.EmployeeCode);
        Assert.Equal("John Doe", employee.ProfileDetails);
        Assert.False(employee.AdminFlag);
        Assert.NotEqual(Guid.Empty, employee.Id);
        Assert.Empty(employee.FaceEmbeddings);
    }

    [Fact]
    public void AddFaceEmbedding_WhenLimitNotReached_ShouldAddEmbedding()
    {
        // Arrange
        var employee = new Employee("EMP-001", "John Doe", false);
        var vector = new float[] { 0.1f, 0.2f, 0.3f };

        // Act
        employee.AddFaceEmbedding(vector);

        // Assert
        Assert.Single(employee.FaceEmbeddings);
    }

    [Fact]
    public void AddFaceEmbedding_WhenLimitReached_ShouldThrowException()
    {
        // Arrange
        var employee = new Employee("EMP-001", "John Doe", false);
        for (int i = 0; i < 5; i++)
        {
            employee.AddFaceEmbedding(new float[] { 0.1f, 0.2f, 0.3f });
        }

        // Act & Assert
        var exception = Assert.Throws<MaxFaceEmbeddingsReachedException>(() => 
            employee.AddFaceEmbedding(new float[] { 0.4f, 0.5f, 0.6f })
        );

        Assert.Equal("An employee can have a maximum of 5 face embeddings.", exception.Message);
    }
}
