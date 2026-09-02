# Handoff: NETFace Attendance backend foundation

## Focus for the next agent

Continue backend planning and implementation for the employee-attendance system. This handoff is for Antigravity, which owns future backend work. Codex owns the future React frontend. Agents operate sequentially, not concurrently.

## Project and stack

The project is an employee-attendance system with future face-recognition support.

- Backend: ASP.NET Core Web API on .NET 8
- Intended persistence: Entity Framework Core + PostgreSQL
- Frontend: React + Vite + JavaScript
- Backend owner: Antigravity
- Frontend owner: Codex

EF Core/Npgsql integration is intentionally not present yet because persistence behavior and the final schema have not been specified.

## Repository structure

```text
NETFace.Attendance.sln
src/backend/
  NETFace.Attendance.Api/            empty ASP.NET Core host
  NETFace.Attendance.Application/    empty application layer
  NETFace.Attendance.Domain/         empty domain layer
  NETFace.Attendance.Infrastructure/ empty adapter/persistence layer
src/frontend/netface-attendance-web/ React + Vite skeleton
tests/NETFace.Attendance.Api.Tests/  xUnit test project (no behavior tests yet)
docs/adr/                            architecture decisions
docs/agents/                         Matt Pocock workflow configuration
```

Dependency direction is `Api -> Application -> Domain`; `Infrastructure -> Application, Domain`; and `Api -> Infrastructure`.

## Architecture decisions already made

See `docs/adr/0001-repository-technology-boundaries.md` for the recorded technology and ownership boundary. The repository uses a single-context domain layout. Local Markdown is the issue tracker under `.scratch/<feature>/`; see `docs/agents/issue-tracker.md`. Domain-doc consumer rules are in `docs/agents/domain.md`.

## Domain terminology

The canonical glossary is `CONTEXT.md`; use it rather than inventing synonyms. Important terms are `Employee`, `Employee code`, `Face registration`, `Face embedding`, `Attendance session`, `Attendance entry`, `Attendance status`, `Attendance statistics`, `Demo session`, and `Recognition log`. The status vocabulary is `EmployeeStatus` (`Active`, `Inactive`) and `AttendanceStatus` (`Present`, `Absent`, `Late`) as stated requirements, but no code has been created for them.

## Important files

- `AGENTS.md`: agent workflow pointers
- `README.md`: scope, structure, ownership, and commands
- `CONTEXT.md`: domain glossary
- `docs/adr/0001-repository-technology-boundaries.md`: technology/ownership decision
- `docs/agents/issue-tracker.md`: local issue tracker convention
- `docs/agents/domain.md`: domain documentation consumption rules
- `src/backend/NETFace.Attendance.Api/Program.cs`: currently an empty host with no endpoint mapping
- `src/frontend/netface-attendance-web/src/App.jsx`: static foundation placeholder only
- `NETFace.Attendance.sln`: solution file
- `global.json`: SDK 8.0.424 pin

## Completed

- Initialized the Git repository and solution.
- Scaffolded the four backend projects and their project references.
- Scaffolded the React/Vite JavaScript frontend and generated `package-lock.json`.
- Added root `.gitignore` for .NET and JavaScript artifacts.
- Removed template WeatherForecast endpoint, Vite demo interactions, and placeholder test.
- Added the glossary, ADR, README, and agent workflow configuration.

## Not done (intentional)

- No attendance business logic or API endpoints.
- No employee/attendance/recognition entities, enums, DTOs, repositories, or domain services.
- No `IFaceDetectionService`, `IFaceEmbeddingExtractor`, `IFaceMatchingService`, `IEmployeeRepository`, or `IAttendanceSessionRepository` implementations/contracts.
- No EF `DbContext`, Npgsql configuration, migrations, or final PostgreSQL schema.
- No authentication, authorization, face-recognition algorithm, or frontend feature.

## Verification status

- `dotnet build NETFace.Attendance.sln`: passed previously with 0 warnings and 0 errors.
- `dotnet test NETFace.Attendance.sln`: test infrastructure ran; no tests are available because no behavior has been implemented yet.
- `npm run lint`: passed.
- `npm run build`: passed.
- `git diff --check`: passed.

## Git state

- Branch: `main`, tracking `origin/main`
- Working tree: clean at handoff
- Last commit: `e122232fd82d598a3e83818ec53647846d9bce0c` (`feat: initialize project structure with foundational agentic skills and full-stack architecture`)

## Important constraints

Do not infer a final persistence schema from the initial domain list. Do not add business endpoints, attendance rules, face-recognition algorithms, or backend features in the foundation phase. Preserve the Antigravity/Codex ownership boundary and sequential-agent workflow. Read `AGENTS.md`, `CONTEXT.md`, and applicable ADRs before changes.

## Safest next action

Inspect the backend requirements with `/grill-with-docs` (and `/domain-modeling` when terms or decisions need sharpening). Resolve persistence and API boundaries first, record only genuinely hard-to-reverse choices as ADRs, then use `/to-spec` and `/to-tickets` before implementing a bounded backend ticket with `/implement` and tests.

## Suggested skills

- `grill-with-docs`
- `domain-modeling`
- `to-spec`
- `to-tickets`
- `tdd`
- `implement`
- `code-review`
