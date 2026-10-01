using FluentAssertions;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain;

public class EventStatusRulesTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);

    [Theory]
    [InlineData("2026-10-02 18:00", "2026-10-02 22:00", EventStatus.Upcoming)]
    [InlineData("2026-10-01 18:00", "2026-10-01 22:00", EventStatus.Ongoing)]
    [InlineData("2026-09-30 20:00", "2026-10-01 02:00", EventStatus.Ongoing)]
    [InlineData("2026-09-30 18:00", "2026-09-30 22:00", EventStatus.Completed)]
    public void Resolve_UsesTheAcademysDay(string starts, string ends, EventStatus expected)
    {
        EventStatusRules.Resolve(false, DateTime.Parse(starts), DateTime.Parse(ends), Today)
            .Should().Be(expected);
    }

    [Fact]
    public void Resolve_Cancelled_WinsOverDates()
    {
        EventStatusRules.Resolve(true, new DateTime(2026, 10, 5), new DateTime(2026, 10, 5, 4, 0, 0), Today)
            .Should().Be(EventStatus.Cancelled);
    }
}
