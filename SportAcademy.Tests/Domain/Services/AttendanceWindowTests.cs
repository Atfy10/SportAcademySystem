using FluentAssertions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain.Services;

// The rule: attendance can be recorded from the session's start until midnight (12:00 AM) at
// the end of that same day - tenant wall-clock time throughout.
public class AttendanceWindowTests
{
    private static readonly DateTime Start = new(2026, 9, 21, 17, 30, 0); // a Monday evening session

    [Fact]
    public void ClosesAt_IsMidnightAtTheEndOfTheSessionsOwnDay()
    {
        AttendanceWindow.ClosesAt(Start).Should().Be(new DateTime(2026, 9, 22, 0, 0, 0));
    }

    [Fact]
    public void IsOpen_FromTheExactStartOfTheSession()
    {
        AttendanceWindow.IsOpen(Start, Start).Should().BeTrue();
    }

    [Fact]
    public void IsOpen_IsFalseBeforeTheSessionStarts()
    {
        AttendanceWindow.IsOpen(Start.AddSeconds(-1), Start).Should().BeFalse();
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 21, 9, 0, 0), Start).Should().BeFalse();
    }

    [Theory]
    [InlineData(19, 0, 0)]   // right after a ~1h session ends - already open before, still open
    [InlineData(21, 45, 0)]  // hours after it ended - used to be closed (only 90 minutes of grace)
    [InlineData(23, 59, 59)] // the last second of the day
    public void IsOpen_StaysOpenAllTheWayToMidnight(int hour, int minute, int second)
    {
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 21, hour, minute, second), Start).Should().BeTrue();
    }

    [Fact]
    public void IsOpen_ClosesAtMidnightSharp_AndStaysClosedAfterwards()
    {
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 22, 0, 0, 0), Start).Should().BeFalse();
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 22, 0, 0, 1), Start).Should().BeFalse();
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 22, 8, 0, 0), Start).Should().BeFalse();
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 23, 17, 30, 0), Start).Should().BeFalse();
    }

    [Fact]
    public void IsOpen_ASessionThatRunsPastMidnightStillClosesAtThatMidnight()
    {
        var lateStart = new DateTime(2026, 9, 21, 23, 30, 0);

        AttendanceWindow.IsOpen(new DateTime(2026, 9, 21, 23, 59, 0), lateStart).Should().BeTrue();
        AttendanceWindow.IsOpen(new DateTime(2026, 9, 22, 0, 1, 0), lateStart).Should().BeFalse();
    }

    [Fact]
    public void IsOpen_EarlyMorningSessionIsOpenUntilThatNightsMidnight()
    {
        var early = new DateTime(2026, 9, 21, 6, 0, 0);

        AttendanceWindow.IsOpen(new DateTime(2026, 9, 21, 22, 0, 0), early).Should().BeTrue();
    }
}
