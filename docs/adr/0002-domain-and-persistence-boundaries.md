# Domain and Persistence Boundaries

## Context and Problem Statement
Before implementing the backend foundation for the employee-attendance system, we needed to establish clear boundaries for domain aggregates, persistence strategies, lifecycle states, and API interaction models. Making ad-hoc decisions during implementation risks creating unmaintainable domain dependencies and tightly coupled systems.

## Considered Options
* Direct database updates for attendance (CRUD-style) vs Aggregate-driven operations.
* Deleting records on session cancellation vs Soft cancellation and history retention.
* Generating attendance entries dynamically on facial recognition vs Pre-loading the roster.
* Directly saving face embeddings in `pgvector` vs In-memory abstraction.
* Passing features extracted by clients vs Uploading raw images to the backend.

## Decision Outcome
We have adopted a strict Domain-Driven Design (DDD) approach with the following boundaries:

1. **Aggregate Roots**: `Employee` and `AttendanceSession` are the primary aggregate roots. `FaceEmbedding` is an entity owned entirely by `Employee` (cascade ownership). `AttendanceEntry` is owned entirely by `AttendanceSession`.
2. **History and Snapshotting**: `AttendanceEntry` references an `EmployeeId` but stores snapshots of `EmployeeCode` and `EmployeeName` to preserve historical integrity if employee data changes. Soft cancellation (`Cancelled` state) is used for `AttendanceSession` instead of physically deleting entries.
3. **Roster Pre-loading**: An `AttendanceSession` pre-loads the roster with default `Absent` entries when created. This simplifies statistics and finalize behavior.
4. **Duplicate Recognition**: The first valid recognition in a session counts. Subsequent recognitions are ignored for attendance marking but logged in the `RecognitionLog`.
5. **Finalize Behavior**: Finalizing a session locks it, freezing the `Absent` status for all unmarked employees. Re-opening is forbidden through normal workflows.
6. **Face Matching Strategy**: `IFaceMatchingService` will use in-memory vector matching for version 1. `pgvector` is not used yet, ensuring a clean abstraction layer for future upgrades.
7. **API Boundary and Auth**: The API accepts raw images (HTTP POST) and performs all processing. Auth uses JWT for both admins and terminals. Terminals will have their own dedicated login flow to acquire a JWT with a 'Device' role. Real-time streaming (WebSocket/gRPC) is excluded from version 1.

## Consequences
* **Positive**: Audit history is completely immutable and protected. The domain model prevents inconsistent states (e.g., hanging attendance entries). Statistics calculations are simplified via roster pre-loading.
* **Negative/Trade-offs**: In-memory face matching may have scaling limitations requiring transition to `pgvector` later. Pre-loading rosters means a slightly higher initial insert cost when creating sessions.
