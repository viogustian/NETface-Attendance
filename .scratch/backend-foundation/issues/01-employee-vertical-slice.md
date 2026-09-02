Status: unclaimed
Type: task
Blocked by: 

# 01 - Employee Vertical Slice

## Objective
Implement the `Employee` and `FaceEmbedding` domain entities, set up the EF Core `DbContext`, and expose a basic REST API for creating and listing employees.

## Scope
- Create `EmployeeStatus` (Active, Inactive) enum.
- Create `Employee` entity with standard properties (`EmployeeCode`, `ProfileDetails`, `Status`, `AdminFlag`).
- Create `FaceEmbedding` entity (`Vector`, `CapturedAt`).
- Implement domain logic ensuring a maximum of 5 embeddings per employee.
- Configure Entity Framework Core `DbContext` in the Infrastructure layer.
- Create REST API endpoints (Controller) to Create an Employee and List Employees.

## Acceptance Criteria
- [ ] `Employee` and `FaceEmbedding` exist in the `Domain` layer without DB dependencies.
- [ ] Attempting to add a 6th embedding to an Employee throws a Domain Exception.
- [ ] `DbContext` is correctly configured with `DbSet`s and constraints.
- [ ] `POST /api/employees` successfully creates an employee.
- [ ] `GET /api/employees` returns a list of employees.

## Files/Modules Affected
- `NETFace.Attendance.Domain` (Entities, Enums, Exceptions)
- `NETFace.Attendance.Infrastructure` (DbContext, EF Configuration)
- `NETFace.Attendance.Api` (Controllers, DTOs)

## Test Strategy
- Unit tests for domain logic (Max 5 embeddings limit, Active default status).
- Basic API tests (in-memory test server) for the endpoints.

## Comments
*(Conversation history appends here)*
