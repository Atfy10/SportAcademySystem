using FluentAssertions;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Domain.Services;

public class SubscriptionDetailsServiceTests
{
    [Fact]
    public void HasActiveSubscriptionConflict_NullActiveList_ReturnsFalse()
    {
        var sub = CreateSubDetails(sportId: 1, start: "2026-01-01", end: "2026-01-31");

        SubscriptionDetailsService.HasActiveSubscriptionConflict(sub, null)
            .Should().BeFalse();
    }

    [Fact]
    public void HasActiveSubscriptionConflict_EmptyActiveList_ReturnsFalse()
    {
        var sub = CreateSubDetails(sportId: 1, start: "2026-01-01", end: "2026-01-31");

        SubscriptionDetailsService.HasActiveSubscriptionConflict(sub, [])
            .Should().BeFalse();
    }

    [Fact]
    public void HasActiveSubscriptionConflict_OverlappingSameSport_ReturnsTrue()
    {
        var sub = CreateSubDetails(sportId: 1, start: "2026-01-15", end: "2026-02-15");
        var activeList = new List<SubscriptionDetails>
        {
            CreateSubDetails(sportId: 1, start: "2026-01-01", end: "2026-01-31")
        };

        SubscriptionDetailsService.HasActiveSubscriptionConflict(sub, activeList)
            .Should().BeTrue();
    }

    [Fact]
    public void HasActiveSubscriptionConflict_OverlappingDifferentSport_ReturnsFalse()
    {
        var sub = CreateSubDetails(sportId: 1, start: "2026-01-15", end: "2026-02-15");
        var activeList = new List<SubscriptionDetails>
        {
            CreateSubDetails(sportId: 2, start: "2026-01-01", end: "2026-01-31")
        };

        SubscriptionDetailsService.HasActiveSubscriptionConflict(sub, activeList)
            .Should().BeFalse();
    }

    [Fact]
    public void CalculateAllowedSessions_ReturnsDaysPerMonth()
    {
        var sub = new SubscriptionDetails
        {
            StartDate = DateOnly.Parse("2026-01-01"),
            EndDate = DateOnly.Parse("2026-01-31"),
            PaymentNumber = "PAY-001",
            SportPrice = new SportPrice
            {
                SportSubscriptionType = new SportSubscriptionType
                {
                    SubscriptionType = new SubscriptionType
                    {
                        Name = SubType.Gold,
                        DaysPerMonth = 16
                    }
                }
            }
        };

        SubscriptionDetailsService.CalculateAllowedSessions(sub).Should().Be(16);
    }

    private static SubscriptionDetails CreateSubDetails(int sportId, string start, string end) =>
        new()
        {
            SportId = sportId,
            StartDate = DateOnly.Parse(start),
            EndDate = DateOnly.Parse(end),
            PaymentNumber = "PAY-TEST"
        };
}
