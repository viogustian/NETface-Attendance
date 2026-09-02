# Backend Foundation Map

This map tracks the progression of vertical slice tickets for the NETFace Attendance backend foundation.

## Decisions so far
- System bounded into strict DDD layers (Api, Application, Domain, Infrastructure).
- Employee and AttendanceSession are aggregate roots.
- AttendanceSession preloads absent entries to simplify statistics.
- Face matching uses in-memory Euclidean distance for v1 (dummy).

## Tickets

1. `[01-employee-vertical-slice.md]` - Employee & FaceEmbedding Domain + Admin CRUD API
2. `[02-session-vertical-slice.md]` - AttendanceSession Domain + Preload + Admin Session API
3. `[03-dummy-recognition-services.md]` - Recognition Interfaces & Dummy Euclidean Implementation
4. `[04-attendance-marking-vertical-slice.md]` - Device Marking API + RecognitionLog
5. `[05-authentication-authorization.md]` - Admin JWT & Device API Key Auth
