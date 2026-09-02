# NETFace Attendance

Repository foundation for an employee-attendance system with future face-recognition support.

## Status

Only repository initialization is complete. No attendance rules, recognition algorithm, business API endpoint, Entity Framework `DbContext`, database migration, or final PostgreSQL schema has been implemented.

## Structure

```text
src/
  backend/
    NETFace.Attendance.Api/            ASP.NET Core host; no business endpoints
    NETFace.Attendance.Application/    Future use cases and ports
    NETFace.Attendance.Domain/         Future domain model and contracts
    NETFace.Attendance.Infrastructure/ Future persistence and external adapters
  frontend/
    netface-attendance-web/            React + Vite JavaScript skeleton
tests/
  NETFace.Attendance.Api.Tests/        Backend test project
docs/
  adr/                                 Architecture decisions
  agents/                              Workflow configuration
```

Dependency direction is `Api -> Application -> Domain`; `Infrastructure -> Application, Domain`; and `Api -> Infrastructure`. This prevents delivery, use-case, domain, and adapter concerns from being mixed.

## Ownership

- Antigravity owns future backend implementation.
- Codex owns future frontend implementation.
- Agents work one at a time. Read `CONTEXT.md`, applicable ADRs, and `AGENTS.md` before changing a feature area.

## Local commands

```powershell
dotnet build NETFace.Attendance.sln
dotnet test NETFace.Attendance.sln
cd src/frontend/netface-attendance-web
npm install
npm run dev
```

The frontend dependency lockfile is committed with the frontend foundation for reproducible installs.
