using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NETFace.Attendance.Api.Controllers.Employees;
using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Infrastructure.Persistence;

namespace NETFace.Attendance.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request)
    {
        var employee = new Employee(request.EmployeeCode, request.FullName, request.IsAdmin);

        db.Employees.Add(employee);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            return Conflict(new { message = $"Employee code '{request.EmployeeCode}' already exists." });
        }

        var response = ToResponse(employee);
        return CreatedAtAction(nameof(Get), new { id = employee.Id }, response);
    }

    [HttpGet("{id:guid}", Name = nameof(Get))]
    public async Task<IActionResult> Get(Guid id)
    {
        var employee = await db.Employees
            .Include(e => e.FaceEmbeddings)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee is null)
            return NotFound();

        return Ok(ToResponse(employee));
    }

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var employees = await db.Employees
            .Select(e => ToResponse(e))
            .ToListAsync();

        return Ok(employees);
    }

    private static EmployeeResponse ToResponse(Employee e) =>
        new(e.Id, e.EmployeeCode, e.FullName, e.IsAdmin, e.Status.ToString());
}
