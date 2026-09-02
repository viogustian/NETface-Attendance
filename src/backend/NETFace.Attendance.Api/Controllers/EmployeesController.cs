using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Api.DTOs;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEmployeeRequest request)
    {
        var employee = new Employee(request.EmployeeCode, request.ProfileDetails, request.AdminFlag);
        
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var response = new EmployeeResponse(
            employee.Id,
            employee.EmployeeCode,
            employee.ProfileDetails,
            employee.Status.ToString(),
            employee.AdminFlag,
            employee.FaceEmbeddings.Count
        );

        return CreatedAtAction(nameof(Get), new { id = employee.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var employee = await _context.Employees
            .Include(e => e.FaceEmbeddings)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null) return NotFound();

        var response = new EmployeeResponse(
            employee.Id,
            employee.EmployeeCode,
            employee.ProfileDetails,
            employee.Status.ToString(),
            employee.AdminFlag,
            employee.FaceEmbeddings.Count
        );

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var employees = await _context.Employees
            .Include(e => e.FaceEmbeddings)
            .Select(e => new EmployeeResponse(
                e.Id,
                e.EmployeeCode,
                e.ProfileDetails,
                e.Status.ToString(),
                e.AdminFlag,
                e.FaceEmbeddings.Count
            ))
            .ToListAsync();

        return Ok(employees);
    }
}
