using FluentAssertions;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain.Services;

public class SubscriptionRulesAndAttendanceRateTests
{
    private static SubscriptionDetails Sub(int id, int sportId, string start, string end,
        SubscriptionStatus status = SubscriptionStatus.Active) => new()
    {
        Id = id,
        SportId = sportId,
        StartDate = DateOnly.Parse(start),
        EndDate = DateOnly.Parse(end),
        Status = status,
    };

    // ── One subscription per sport; a new one starts after the latest ends ─────────────

    [Fact]
    public void NewSubscription_StartingBeforeAnUpcomingOne_InTheSameSport_Conflicts()
    {
        var upcoming = Sub(1, sportId: 5, "2026-11-01", "2026-11-30");
        var candidate = Sub(0, sportId: 5, "2026-10-01", "2026-10-25");

        SubscriptionDetailsService.FindSameSportConflict(candidate, [upcoming])
            .Should().BeSameAs(upcoming);
    }

    [Fact]
    public void NewSubscription_OverlappingASuspendedOne_Conflicts()
    {
        var suspended = Sub(1, sportId: 5, "2026-09-01", "2026-09-30", SubscriptionStatus.Suspended);
        var candidate = Sub(0, sportId: 5, "2026-09-20", "2026-10-20");

        SubscriptionDetailsService.FindSameSportConflict(candidate, [suspended]).Should().NotBeNull();
    }

    [Fact]
    public void NewSubscription_StartingTheDayAfterTheLatestEnds_IsAllowed()
    {
        var current = Sub(1, sportId: 5, "2026-09-01", "2026-09-30");
        var candidate = Sub(0, sportId: 5, "2026-10-01", "2026-10-31");

        SubscriptionDetailsService.FindSameSportConflict(candidate, [current]).Should().BeNull();
    }

    [Fact]
    public void Conflict_ReportsTheSubscriptionEndingLast()
    {
        var current = Sub(1, sportId: 5, "2026-09-01", "2026-09-30");
        var renewal = Sub(2, sportId: 5, "2026-10-01", "2026-10-31");
        var candidate = Sub(0, sportId: 5, "2026-09-15", "2026-10-15");

        SubscriptionDetailsService.FindSameSportConflict(candidate, [current, renewal])
            .Should().BeSameAs(renewal);
    }

    [Fact]
    public void OtherSport_AndTheSubscriptionItself_NeverConflict()
    {
        var otherSport = Sub(1, sportId: 6, "2026-09-01", "2026-09-30");
        var self = Sub(2, sportId: 5, "2026-09-01", "2026-09-30");

        SubscriptionDetailsService.FindSameSportConflict(self, [otherSport, self]).Should().BeNull();
    }

    // ── Upcoming becomes Active on the academy's own start date ─────────────────────────

    [Fact]
    public void StartingToday_IsActive_StartingTomorrow_IsUpcoming()
    {
        var today = new DateOnly(2026, 10, 1);

        SubscriptionBilling.EffectiveStatus(Sub(1, 5, "2026-10-01", "2026-10-31"), today)
            .Should().Be(SubscriptionStatus.Active);
        SubscriptionBilling.EffectiveStatus(Sub(1, 5, "2026-10-02", "2026-10-31"), today)
            .Should().Be(SubscriptionStatus.Upcoming);
    }

    [Fact]
    public void TenantCalendar_UsesTheAcademysTimeZone_NotUtc()
    {
        TenantCalendar.Set(new DateOnly(2026, 10, 1));
        try
        {
            TenantCalendar.Today.Should().Be(new DateOnly(2026, 10, 1));
        }
        finally
        {
            TenantCalendar.Set(null);
        }
    }

    // ── Attendance rate ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(7, 10, 70.0)]     // was 0 with integer division
    [InlineData(1, 3, 33.3)]
    [InlineData(10, 10, 100.0)]
    [InlineData(0, 0, 0.0)]       // nothing marked yet
    public void AttendanceRate_IsAPercentageWithOneDecimal(int attended, int counted, double expected)
        => AttendanceRate.Percent(attended, counted).Should().Be(expected);

    [Fact]
    public void Late_CountsAsAttended_Excused_IsNotCountedAgainstTheTrainee()
    {
        AttendanceRate.IsAttended(AttendanceStatus.Late).Should().BeTrue();
        AttendanceRate.IsAttended(AttendanceStatus.Absent).Should().BeFalse();
        AttendanceRate.IsCounted(AttendanceStatus.Excused).Should().BeFalse();
        AttendanceRate.IsCounted(AttendanceStatus.Absent).Should().BeTrue();
    }
}
