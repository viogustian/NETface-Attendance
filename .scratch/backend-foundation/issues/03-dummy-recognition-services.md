Status: unclaimed
Type: task
Blocked by: 01

# 03 - Dummy Recognition Services

## Objective
Define the application layer abstractions for facial recognition and provide a dummy implementation in the infrastructure layer using simple Euclidean distance arrays for testing purposes.

## Scope
- Define `IFaceDetectionService`, `IFaceEmbeddingExtractor`, and `IFaceMatchingService` in `NETFace.Attendance.Application`.
- Implement a dummy Euclidean distance matcher in `NETFace.Attendance.Infrastructure`.
- The dummy matcher will compute distance between vectors to simulate a match or mismatch.

## Acceptance Criteria
- [ ] Application layer strictly references interfaces, having no ML/AI dependencies.
- [ ] Dummy implementations accurately compute match thresholds based on a configurable distance limit.
- [ ] Unit tests verify Euclidean distance math.

## Files/Modules Affected
- `NETFace.Attendance.Application` (Interfaces)
- `NETFace.Attendance.Infrastructure` (Dummy implementations)

## Dependencies & Blockers
- Blocked by `01-employee-vertical-slice` (Requires `FaceEmbedding` model).

## Test Strategy
- Unit test passing identical vectors (Match).
- Unit test passing drastically different vectors (No-Match).

## Comments
*(Conversation history appends here)*
