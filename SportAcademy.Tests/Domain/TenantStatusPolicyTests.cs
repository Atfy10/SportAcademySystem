using FluentAssertions;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Domain;

public class TenantStatusPolicyTests
{
    public static IEnumerable<object[]> PermittedTransitions =>
    [
        [TenantStatus.PendingSetup, TenantStatus.Suspended],
        [TenantStatus.Active, TenantStatus.Suspended],
        [TenantStatus.Active, TenantStatus.Inactive],
        [TenantStatus.Suspended, TenantStatus.Active],
        [TenantStatus.Inactive, TenantStatus.Active],
        [TenantStatus.Suspended, TenantStatus.Archived],
        [TenantStatus.Inactive, TenantStatus.Archived],
        [TenantStatus.Archived, TenantStatus.Suspended],
    ];

    [Theory]
    [MemberData(nameof(PermittedTransitions))]
    public void CanTransition_PermittedPair_ReturnsTrue(TenantStatus from, TenantStatus to)
    {
        TenantStatusPolicy.CanTransition(from, to).Should().BeTrue();
    }

    // Every (from, to) pair not in PermittedTransitions must be false - this is what keeps
    // ChangeTenantStatusCommandHandler and ArchiveTenantCommandHandler from ever disagreeing
    // again the way they did before both routed through this policy (F-03/F-04).
    public static IEnumerable<object[]> AllStatusPairs()
    {
        var statuses = Enum.GetValues<TenantStatus>();
        foreach (var from in statuses)
            foreach (var to in statuses)
                yield return [from, to];
    }

    [Theory]
    [MemberData(nameof(AllStatusPairs))]
    public void CanTransition_AgreesWithPermittedSet(TenantStatus from, TenantStatus to)
    {
        var expected = PermittedTransitions.Any(p => (TenantStatus)p[0] == from && (TenantStatus)p[1] == to);

        TenantStatusPolicy.CanTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void CanTransition_SameStatus_ReturnsFalse()
    {
        foreach (var status in Enum.GetValues<TenantStatus>())
            TenantStatusPolicy.CanTransition(status, status).Should().BeFalse();
    }

    [Fact]
    public void CanTransition_FromArchived_NeverGoesDirectlyToActive()
    {
        // A restored tenant must pass through Suspended (a deliberate second step), not resume
        // Active automatically.
        TenantStatusPolicy.CanTransition(TenantStatus.Archived, TenantStatus.Active).Should().BeFalse();
    }

    [Fact]
    public void CanTransition_ArchivedIsNoLongerFullyTerminal()
    {
        TenantStatusPolicy.CanTransition(TenantStatus.Archived, TenantStatus.Suspended).Should().BeTrue();
    }
}
