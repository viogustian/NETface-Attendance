using NETFace.Attendance.Domain.Entities;
using NETFace.Attendance.Domain.Enums;
using NETFace.Attendance.Domain.Exceptions;

namespace NETFace.Attendance.Api.Tests.Domain;

public class AttendanceSessionTests
{
    // --- Seam: AttendanceSession public interface ---

    private static List<(Guid EmployeeId, string EmployeeCode, string EmployeeName)> BuildRoster(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => (Guid.NewGuid(), $"EMP{i:D3}", $"Employee {i}"))
            .ToList();
    }

    [Fact]
    public void Create_ShouldPreloadAbsentEntryForEachEmployee()
    {
        var roster = BuildRoster(3);

        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);

        Assert.Equal(3, session.Entries.Count);
        Assert.All(session.Entries, e => Assert.Equal(AttendanceStatus.Absent, e.Status));
    }

    [Fact]
    public void Create_ShouldSnapshotEmployeeCodeAndName()
    {
        var employeeId = Guid.NewGuid();
        var roster = new List<(Guid, string, string)> { (employeeId, "EMP001", "Alice Wonderland") };

        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);

        var entry = Assert.Single(session.Entries);
        Assert.Equal(employeeId, entry.EmployeeId);
        Assert.Equal("EMP001", entry.EmployeeCode);
        Assert.Equal("Alice Wonderland", entry.EmployeeName);
    }

    [Fact]
    public void Finalize_ShouldSetStatusToFinalized()
    {
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), BuildRoster(1));

        session.FinalizeSession();

        Assert.Equal(AttendanceSessionStatus.Finalized, session.Status);
    }

    [Fact]
    public void Finalize_WhenAlreadyFinalized_ShouldThrowException()
    {
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), BuildRoster(1));
        session.FinalizeSession();

        Assert.Throws<AttendanceSessionAlreadyFinalizedException>(() => session.FinalizeSession());
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelled_AndRetainEntries()
    {
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), BuildRoster(2));

        session.Cancel();

        Assert.Equal(AttendanceSessionStatus.Cancelled, session.Status);
        Assert.Equal(2, session.Entries.Count);
    }

    [Fact]
    public void Cancel_WhenAlreadyFinalized_ShouldThrowException()
    {
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), BuildRoster(1));
        session.FinalizeSession();

        Assert.Throws<AttendanceSessionAlreadyFinalizedException>(() => session.Cancel());
    }

    [Fact]
    public void MarkAttendance_WhenAbsent_ShouldSetStatusToPresentAndSetMarkedAt()
    {
        var employeeId = Guid.NewGuid();
        var roster = new List<(Guid, string, string)> { (employeeId, "EMP001", "Alice Wonderland") };
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
        var timestamp = new DateTimeOffset(2026, 9, 3, 10, 0, 0, TimeSpan.Zero);

        var marked = session.MarkAttendance(employeeId, timestamp);

        Assert.True(marked);
        var entry = Assert.Single(session.Entries);
        Assert.Equal(AttendanceStatus.Present, entry.Status);
        Assert.Equal(timestamp, entry.MarkedAt);
    }

    [Fact]
    public void MarkAttendance_WhenAlreadyPresent_ShouldNotUpdateMarkedAt()
    {
        var employeeId = Guid.NewGuid();
        var roster = new List<(Guid, string, string)> { (employeeId, "EMP001", "Alice Wonderland") };
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
        var firstTime = new DateTimeOffset(2026, 9, 3, 10, 0, 0, TimeSpan.Zero);
        var secondTime = new DateTimeOffset(2026, 9, 3, 10, 15, 0, TimeSpan.Zero);

        session.MarkAttendance(employeeId, firstTime);
        var secondMarked = session.MarkAttendance(employeeId, secondTime);

        Assert.False(secondMarked);
        var entry = Assert.Single(session.Entries);
        Assert.Equal(AttendanceStatus.Present, entry.Status);
        Assert.Equal(firstTime, entry.MarkedAt);
    }

    [Fact]
    public void MarkAttendance_WhenSessionFinalized_ShouldThrowAttendanceSessionAlreadyFinalizedException()
    {
        var employeeId = Guid.NewGuid();
        var roster = new List<(Guid, string, string)> { (employeeId, "EMP001", "Alice Wonderland") };
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
        session.FinalizeSession();

        Assert.Throws<AttendanceSessionAlreadyFinalizedException>(() =>
            session.MarkAttendance(employeeId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MarkAttendance_WhenSessionCancelled_ShouldThrowInvalidOperationException()
    {
        var employeeId = Guid.NewGuid();
        var roster = new List<(Guid, string, string)> { (employeeId, "EMP001", "Alice Wonderland") };
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), roster);
        session.Cancel();

        Assert.Throws<InvalidOperationException>(() =>
            session.MarkAttendance(employeeId, DateTimeOffset.UtcNow));
    }


    [Fact]
    public void MarkAttendance_WhenEmployeeNotInRoster_ShouldReturnFalse()
    {
        var session = AttendanceSession.Create("Engineering", DateOnly.FromDateTime(DateTime.Today), BuildRoster(1));

        var result = session.MarkAttendance(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.False(result);
    }
}

