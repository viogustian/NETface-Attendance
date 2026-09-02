Status: unclaimed
Type: task
Blocked by: 02, 03

# 04 - Attendance Marking Vertical Slice

## Objective
Implement the core Device API endpoint to accept an image, perform recognition via dummy services, log the attempt, and record attendance.

## Scope
- Create `RecognitionLog` entity and add to EF Core `DbContext`.
- Implement `POST /api/recognition/attempt`.
- Workflow:
  1. Receive raw image.
  2. Detect face -> Extract embedding -> Match against active session & active employees.
  3. If matched, create `RecognitionLog` (Success).
  4. Find `AttendanceEntry`. If already `Present`, ignore. If `Absent`, mark as `Present` (set `MarkedAt`).
  5. If no match / poor image, create `RecognitionLog` (Failed), return graceful response.

## Acceptance Criteria
- [ ] First valid recognition successfully updates `AttendanceEntry` to `Present`.
- [ ] Duplicate recognitions do not update `MarkedAt` but still create a `RecognitionLog`.
- [ ] Failures (e.g. no face detected) return HTTP 200/400 (graceful) and create a Failed `RecognitionLog`.
- [ ] Matches against `Inactive` employees are rejected.

## Files/Modules Affected
- `NETFace.Attendance.Domain` (RecognitionLog)
- `NETFace.Attendance.Application` (Recognition Use Cases)
- `NETFace.Attendance.Api` (Device Controller)
- `NETFace.Attendance.Infrastructure` (DB additions)

## Dependencies & Blockers
- Blocked by `02-session-vertical-slice` (Needs sessions to mark).
- Blocked by `03-dummy-recognition-services` (Needs dummy algorithms).

## Test Strategy
- Integration test simulating device flow: mark once (success), mark again (duplicate handled correctly), mark bad image (failure handled gracefully).

## Comments
*(Conversation history appends here)*
