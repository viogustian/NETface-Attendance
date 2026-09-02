using System;
using System.Collections.Generic;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Domain.Exceptions;

namespace NETFace.Attendance.Domain.Entities;

public class Employee
{
    public Guid Id { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public string ProfileDetails { get; private set; } = string.Empty;
    public EmployeeStatus Status { get; private set; }
    public bool AdminFlag { get; private set; }

    private readonly List<FaceEmbedding> _faceEmbeddings = new();
    public IReadOnlyCollection<FaceEmbedding> FaceEmbeddings => _faceEmbeddings.AsReadOnly();

    // EF Core constructor
    private Employee() { }

    public Employee(string employeeCode, string profileDetails, bool adminFlag)
    {
        Id = Guid.NewGuid();
        EmployeeCode = employeeCode ?? throw new ArgumentNullException(nameof(employeeCode));
        ProfileDetails = profileDetails ?? throw new ArgumentNullException(nameof(profileDetails));
        AdminFlag = adminFlag;
        Status = EmployeeStatus.Active;
    }

    public void AddFaceEmbedding(float[] vector)
    {
        if (_faceEmbeddings.Count >= 5)
        {
            throw new MaxFaceEmbeddingsReachedException("An employee can have a maximum of 5 face embeddings.");
        }
        
        _faceEmbeddings.Add(new FaceEmbedding(this.Id, vector));
    }
}
