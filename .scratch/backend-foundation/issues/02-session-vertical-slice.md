Status: unclaimed
Type: task
Blocked by: 01

# 02 - Attendance Session Vertical Slice

## Objective
Implement the `AttendanceSession` and `AttendanceEntry` domain entities, integrate with EF Core, and expose a REST API to manage sessions.

## Scope
- Create `AttendanceStatus` (Present, Absent, Late) enum.
- Create `AttendanceSession` entity (states: NotStarted, Active, Finalized, Cancelled).
- Create `AttendanceEntry` entity (snapshotting `EmployeeCode` and `EmployeeName`).
- Implement Domain logic: When a session is created, it preloads the roster with `Absent` entries for all provided active employees.
- Implement Domain logic: `Finalize` locks the session; `Cancel` soft-cancels the session but retains entries.
- Create REST API endpoints to Create, Finalize, and Cancel a session.

## Acceptance Criteria
- [ ] `AttendanceSession` correctly generates `AttendanceEntry` children for all provided active employees upon creation.
- [ ] `Finalize` method prevents further status changes to the session.
- [ ] `Cancel` method changes status without cascade physical deletion of entries.
- [ ] API endpoints can trigger these domain actions correctly.

## Files/Modules Affected
- `NETFace.Attendance.Domain` (Entities, Enums)
- `NETFace.Attendance.Infrastructure` (DbContext additions)
- `NETFace.Attendance.Api` (Controllers, DTOs)

## Dependencies & Blockers
- Blocked by `01-employee-vertical-slice` (Requires Employee entity to generate rosters).

## Test Strategy
- Unit tests for roster pre-loading logic (ensuring snapshotting is correct).
- Unit tests for Finalize and Cancel domain rules.

## Comments
*(Conversation history appends here)*
