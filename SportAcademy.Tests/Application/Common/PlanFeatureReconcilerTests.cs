using FluentAssertions;
using SportAcademy.Application.Common.Features;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Common;

public class PlanFeatureReconcilerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void NewlyIncludedFeature_WithNoExistingRow_IsEnabled()
    {
        var featureId = Guid.NewGuid();

        var updates = PlanFeatureReconciler.ComputeUpdates([], [featureId]);

        updates.Should().ContainKey(featureId).WhoseValue.Should().BeTrue();
    }

    [Fact]
    public void NewlyIncludedFeature_WithExistingDisabledRow_IsEnabled()
    {
        var featureId = Guid.NewGuid();
        var current = new List<TenantFeature>
        {
            new() { TenantId = TenantId, FeatureId = featureId, IsEnabled = false },
        };

        var updates = PlanFeatureReconciler.ComputeUpdates(current, [featureId]);

        updates.Should().ContainKey(featureId).WhoseValue.Should().BeTrue();
    }

    [Fact]
    public void NewlyExcludedFeature_WithExistingEnabledRow_IsDisabled()
    {
        var featureId = Guid.NewGuid();
        var current = new List<TenantFeature>
        {
            new() { TenantId = TenantId, FeatureId = featureId, IsEnabled = true },
        };

        var updates = PlanFeatureReconciler.ComputeUpdates(current, []);

        updates.Should().ContainKey(featureId).WhoseValue.Should().BeFalse();
    }

    [Fact]
    public void AlreadyCorrectState_ProducesNoUpdate()
    {
        var includedFeatureId = Guid.NewGuid();
        var excludedFeatureId = Guid.NewGuid();
        var current = new List<TenantFeature>
        {
            new() { TenantId = TenantId, FeatureId = includedFeatureId, IsEnabled = true },
            new() { TenantId = TenantId, FeatureId = excludedFeatureId, IsEnabled = false },
        };

        var updates = PlanFeatureReconciler.ComputeUpdates(current, [includedFeatureId]);

        updates.Should().BeEmpty();
    }

    [Fact]
    public void LockedFeature_IsNeverTouched_EitherDirection()
    {
        var lockedOffButIncludedId = Guid.NewGuid();
        var lockedOnButExcludedId = Guid.NewGuid();
        var current = new List<TenantFeature>
        {
            new() { TenantId = TenantId, FeatureId = lockedOffButIncludedId, IsEnabled = false, LockedBySuperAdmin = true },
            new() { TenantId = TenantId, FeatureId = lockedOnButExcludedId, IsEnabled = true, LockedBySuperAdmin = true },
        };

        var updates = PlanFeatureReconciler.ComputeUpdates(current, [lockedOffButIncludedId]);

        updates.Should().BeEmpty();
    }
}
