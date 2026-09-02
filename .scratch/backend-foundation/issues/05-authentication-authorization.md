Status: unclaimed
Type: task
Blocked by: 01, 02, 04

# 05 - Authentication and Authorization

## Objective
Secure the API by implementing JWT for Administrative endpoints and API Keys/Device Tokens for Terminal (Device) endpoints.

## Scope
- Configure ASP.NET Core Authentication/Authorization middleware.
- Create JWT generation (login) endpoint for Admins.
- Create API Key middleware/filter for Device identification.
- Apply `[Authorize(Roles="Admin")]` to Employee and Session management routes.
- Apply Device authentication to `POST /api/recognition/attempt`.

## Acceptance Criteria
- [ ] Unauthenticated requests to restricted routes return 401 Unauthorized.
- [ ] Admin JWT cannot access Device routes (unless intentionally permitted).
- [ ] Device Token cannot access Admin routes (Employee/Session CRUD).

## Files/Modules Affected
- `NETFace.Attendance.Api` (Middleware, Program.cs, Controllers)

## Dependencies & Blockers
- Blocked by `01`, `02`, `04` (Needs the endpoints to exist before securing them).

## Test Strategy
- Endpoint tests verifying 401, 403, and 200 HTTP statuses based on provided tokens.

## Comments
*(Conversation history appends here)*
