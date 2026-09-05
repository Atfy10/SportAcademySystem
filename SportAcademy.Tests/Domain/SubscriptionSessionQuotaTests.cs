using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain;

/// <summary>
/// The session quota granted to an enrollment covers the whole subscription term, not one month
/// of it. DaysPerMonth is a monthly rate - the seeded "Quarterly" plan is DaysPerMonth 10 over
/// NumberOfMonths 3 - and returning it alone gave a three-month subscriber one month's sessions
/// while they paid for, and were dated for, the full term.
/// </summary>
public class SubscriptionSessionQuotaTests
{
    private static SubscriptionDetails ForPlan(int daysPerMonth, int numberOfMonths)
        => new()
        {
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 4, 1),
            SportPrice = new SportPrice
            {
                SportSubscriptionType = new SportSubscriptionType
                {
                    SubscriptionType = new SubscriptionType
                    {
                        Name = "Test",
                        DaysPerMonth = daysPerMonth,
                        NumberOfMonths = numberOfMonths,
                    },
                },
            },
        };

    [Theory]
    // The seeded plans, by name: Monthly, Quarterly, Silver, Gold, Platinum.
    [InlineData(8, 1, 8)]
    [InlineData(10, 3, 30)]
    [InlineData(12, 1, 12)]
    [InlineData(16, 1, 16)]
    [InlineData(24, 1, 24)]
    public void CalculateAllowedSessions_CoversTheWholeTerm(int daysPerMonth, int months, int expected)
        => Assert.Equal(expected, SubscriptionDetailsService.CalculateAllowedSessions(ForPlan(daysPerMonth, months)));

    [Fact]
    public void CalculateAllowedSessions_MatchesTheFigureTheEndDateIsCountedAcross()
    {
        // The end date walks forward over this many sessions (TrainingScheduleService), so a
        // different quota would leave a trainee out of sessions before their subscription ends,
        // or with sessions left over after it.
        var subscription = ForPlan(10, 3);

        Assert.Equal(
            TrainingScheduleService.CalculateTotalSessions(10, 3),
            SubscriptionDetailsService.CalculateAllowedSessions(subscription));
    }

    [Fact]
    public void CalculateTotalSessions_TreatsAMissingDurationAsOneMonth()
        => Assert.Equal(8, TrainingScheduleService.CalculateTotalSessions(8, 0));
}
