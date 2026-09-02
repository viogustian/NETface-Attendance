# Backend Implementation Specification: Employee Attendance System

## Objective
To build the foundational backend for an employee attendance system utilizing ASP.NET Core Web API, Entity Framework Core, and PostgreSQL, incorporating face recognition capabilities.

## Scope
- Employee management and face embedding registration.
- Attendance session creation and lifecycle management.
- Facial recognition processing (detection, embedding extraction, and matching) in-memory for version 1.
- RESTful API endpoints for devices (terminals) and administrators.
- JWT and Device Token-based authentication.

## Out-of-Scope
- `pgvector` database integration for face matching (deferred to future phases).
- Real-time streaming protocols like WebSockets or gRPC.
- Frontend user interface implementation.
- Complex analytics beyond basic attendance statistics.

## Domain Model
### Aggregate Boundaries
1. **Employee Aggregate**: 
   - **Root**: `Employee`
   - **Child**: `FaceEmbedding`
   - **Constraint**: `FaceEmbedding` cannot exist without an `Employee`. Deleting an employee cascade-deletes their embeddings.
2. **AttendanceSession Aggregate**:
   - **Root**: `AttendanceSession`
   - **Child**: `AttendanceEntry`
   - **Constraint**: `AttendanceEntry` cannot exist independently. Deleting or cancelling a session affects the entries through domain rules, not physical cascade deletes.

### Entity Responsibilities
- **Employee**: Represents a person in the system. Maintains identity (`Id`, `EmployeeCode`, `ProfileDetails`), status (`Active`/`Inactive`), and an administrative flag.
- **FaceEmbedding**: Represents a mathematical vector of a face. Responsible for holding the numeric array and `CapturedAt` timestamp. Hard limit: 5 embeddings per Employee.
- **AttendanceSession**: Represents a departmental attendance period on a specific date. Responsible for pre-loading rosters, tracking state (`NotStarted`, `Active`, `Finalized`, `Cancelled`), and guarding modifications.
- **AttendanceEntry**: Represents an individual's presence record. Stores an `EmployeeId` reference and snapshots of `EmployeeCode` and `EmployeeName`. Stores `AttendanceStatus` (`Present`, `Absent`, `Late`) and `MarkedAt` timestamp.
- **RecognitionLog**: Persisted audit log for recognition attempts, storing matched employee references, confidence scores, and processing times.

### Value Objects
- **DetectedFace**: Transient object representing a localized face within an image.
- **FaceMatchResult**: Transient object representing the outcome of comparing a detected face against known embeddings.
- *Note*: These transient objects only live in memory during an API request and are never persisted to PostgreSQL.

## Repository Responsibilities
- **IEmployeeRepository**: Handles persistence of `Employee` and their `FaceEmbedding`s.
- **IAttendanceSessionRepository**: Handles persistence of `AttendanceSession` and its `AttendanceEntry` children.

## Service Responsibilities
- **IFaceDetectionService**: Extracts `DetectedFace` from a raw image.
- **IFaceEmbeddingExtractor**: Converts a `DetectedFace` into a numeric vector.
- **IFaceMatchingService**: Compares an extracted vector against known `FaceEmbedding`s to produce a `FaceMatchResult`.

## Persistence Design
- **PostgreSQL Decisions**: Standard relational tables using Entity Framework Core.
- **Embeddings**: Stored as standard arrays or JSONB (implementation detail) but retrieved into application memory for matching. `pgvector` extension is **not** used in v1.
- **Soft Cancellation**: Cancelling an `AttendanceSession` does not physically delete `AttendanceEntry` rows.

## API Boundaries
- **Protocol**: Standard REST HTTP APIs.
- **Input**: The `/api/recognition/attempt` endpoint accepts a raw image (JPEG/PNG).
- **Processing**: The API delegates raw image processing entirely to backend services. Clients do not perform detection or matching.

## Authentication Boundary
- **Admin**: Uses JWT (JSON Web Tokens) to access employee management and session management endpoints.
- **Device/Terminal**: Uses specialized Device Tokens or API Keys to access only recognition and attendance marking endpoints.

## Attendance Lifecycle
1. **Pre-loading**: Upon session creation, an `AttendanceEntry` is generated for every targeted employee, defaulting to `Absent`.
2. **Marking**: The first valid face recognition match changes the status to `Present` and sets `MarkedAt`.
3. **Finalization**: `Finalized` state locks the session. No automatic recognitions can modify entries. `Absent` statuses become permanent unless administratively corrected.

## Error Behavior & Edge Cases
- **Duplicate Recognition**: If an employee is recognized multiple times in an active session, the first timestamp (`MarkedAt`) is preserved. The subsequent matches do not overwrite the entry or status, but are still logged in `RecognitionLog`.
- **Recognition Failures**: Poor image quality or no-match scenarios result in a `Failed` `RecognitionLog`. The API returns a non-fatal response allowing the device to continue processing frames.
- **Inactive Employees**: If an `Inactive` employee's face is matched, it is rejected by business rules and does not result in a valid attendance mark.
- **Embedding Limits**: Attempting to add a 6th embedding to an employee throws a domain exception. It does not silently overwrite old embeddings.

## Testing Strategy
- **Unit Testing**: Domain logic (limits, lifecycles, duplicate protection) must be fully covered by unit tests.
- **Mocking**: Face recognition services (`IFaceDetectionService`, etc.) must be abstracted and mocked. A simple Euclidean distance dummy matcher will be used for API and Application layer testing. Tests must not depend on actual ML models.

## Acceptance Criteria
- [ ] Employee can be created, updated, and deactivated.
- [ ] Face embeddings can be registered up to a maximum of 5 per employee.
- [ ] AttendanceSession pre-loads the roster correctly upon creation.
- [ ] API successfully receives raw images and delegates to dummy matching services.
- [ ] First successful face match records attendance; duplicates are ignored for attendance but logged.
- [ ] Finalizing a session prevents further attendance updates.
- [ ] Security correctly isolates Admin JWT access from Device API Key access.
