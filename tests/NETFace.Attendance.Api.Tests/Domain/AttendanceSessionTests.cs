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
}
