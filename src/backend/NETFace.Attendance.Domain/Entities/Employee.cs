using System;
using System.Collections.Generic;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Domain.Exceptions;

namespace NETFace.Attendance.Domain.Entities;

public class Employee
{
    private const int MaxEmbeddings = 5;

    private readonly List<FaceEmbedding> _faceEmbeddings = new();

    public Guid Id { get; private set; }
    public string EmployeeCode { get; private set; }
    public string FullName { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public bool IsAdmin { get; private set; }
    public IReadOnlyCollection<FaceEmbedding> FaceEmbeddings => _faceEmbeddings.AsReadOnly();

    // EF Core constructor
    private Employee() { Id = Guid.NewGuid(); EmployeeCode = string.Empty; FullName = string.Empty; }

    public Employee(string employeeCode, string fullName, bool isAdmin)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(employeeCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        Id = Guid.NewGuid();
        EmployeeCode = employeeCode;
        FullName = fullName;
        IsAdmin = isAdmin;
        Status = EmployeeStatus.Active;
    }

    public void AddFaceEmbedding(float[] vector)
    {
        if (_faceEmbeddings.Count >= MaxEmbeddings)
            throw new MaxFaceEmbeddingsReachedException();

        _faceEmbeddings.Add(new FaceEmbedding(vector));
    }

    public void Deactivate()
    {
        Status = EmployeeStatus.Inactive;
    }

    public void Activate()
    {
        Status = EmployeeStatus.Active;
    }
}

