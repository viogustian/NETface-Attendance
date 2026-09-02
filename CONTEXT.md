# Employee Attendance

This context defines the language of the employee-attendance system. It covers employee identity, attendance records, and face-recognition concepts without prescribing application behavior or persistence.

## People

**Employee**:
A person recorded by the organization for attendance purposes. An employee has an employee code, profile details, employment status, administrative flag, and zero or more face embeddings.
_Avoid_: User, account, staff record

**Employee code**:
The identifier used to refer to an employee in attendance and recognition records.
_Avoid_: User ID, badge ID

**Face registration**:
The state indicating whether an employee has face data registered for recognition.
_Avoid_: Face enrollment, biometric setup

## Attendance

**Attendance session**:
A department-scoped attendance period on a date. It contains attendance entries and has a finalization state.
_Avoid_: Meeting, event

**Attendance entry**:
An attendance record within one attendance session, retaining the employee code and employee name recorded at marking time.
_Avoid_: Check-in, presence record

**Attendance status**:
The recorded result for an attendance entry: `Present`, `Absent`, or `Late`.
_Avoid_: Employee status

**Attendance statistics**:
A summary of employee totals, present and absent counts, and attendance rate.
_Avoid_: Attendance session, report

## Recognition

**Face embedding**:
A captured numeric vector associated with one employee and a capture time.
_Avoid_: Face image, face template

**Demo session**:
A session identified by a session code for capturing recognition logs.
_Avoid_: Attendance session

**Recognition log**:
A timestamped record of a recognition attempt, including its matched employee code, confidence, and processing time.
_Avoid_: Attendance entry, audit event
