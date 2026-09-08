using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

// A plan change's only real "spec" in this system is which features it grants
// (SubscriptionPlanFeature) - these lock in that a downgrade actually revokes anything the
// tenant no longer has entitlement to, an upgrade doesn't force-enable anything the tenant
// hasn't opted into, and a SuperAdmin-locked feature is untouched by plan membership entirely.
public class ChangeTenantPlanCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IBaseRepository<SubscriptionPlan, int>> _planRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ChangeTenantPlanCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();

    public ChangeTenantPlanCommandHandlerTests()
    {
        _handler = new ChangeTenantPlanCommandHandler(_tenantRepoMock.Object, _planRepoMock.Object, _unitOfWorkMock.Object);
    }

    private static Tenant CreateTenantWithSubscription(int currentPlanId) => new()
    {
        Id = TenantId,
        Name = "Test",
        Slug = "test",
        Subscription = new TenantSubscription { TenantId = TenantId, SubscriptionPlanId = currentPlanId },
    };

    [Fact]
    public async Task Handle_Downgrade_RevokesFeaturesNotInTheNewPlan()
    {
        var tenant = CreateTenantWithSubscription(currentPlanId: 2);
        var keptFeatureId = Guid.NewGuid();
        var revokedFeatureId = Guid.NewGuid();
        var lockedFeatureId = Guid.NewGuid();

        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionPlan { Id = 1, Name = "Basic", Code = "BASIC" });
        // Only keptFeatureId survives into the new (cheaper) plan.
        _tenantRepoMock.Setup(r => r.GetPlanFeaturesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync([keptFeatureId]);
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new TenantFeature { TenantId = TenantId, FeatureId = keptFeatureId, IsEnabled = true },
                new TenantFeature { TenantId = TenantId, FeatureId = revokedFeatureId, IsEnabled = true },
                // Locked despite not being in the new plan - a SuperAdmin's forced decision must
                // survive a plan change untouched.
                new TenantFeature { TenantId = TenantId, FeatureId = lockedFeatureId, IsEnabled = true, LockedBySuperAdmin = true },
            ]);

        Dictionary<Guid, bool>? capturedUpdates = null;
        _tenantRepoMock
            .Setup(r => r.BulkUpdateFeaturesAsync(TenantId, It.IsAny<Dictionary<Guid, bool>>(), "PlanChange", It.IsAny<CancellationToken>()))
            .Callback<Guid, Dictionary<Guid, bool>, string, CancellationToken>((_, updates, _, _) => capturedUpdates = updates)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Subscription!.SubscriptionPlanId.Should().Be(1);
        capturedUpdates.Should().NotBeNull();
        capturedUpdates!.Should().ContainKey(revokedFeatureId).WhoseValue.Should().BeFalse();
        capturedUpdates.Should().NotContainKey(keptFeatureId);
        capturedUpdates.Should().NotContainKey(lockedFeatureId);
    }

    [Fact]
    public async Task Handle_Upgrade_DoesNotForceEnableAnything()
    {
        var tenant = CreateTenantWithSubscription(currentPlanId: 1);
        var newlyAvailableFeatureId = Guid.NewGuid();

        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionPlan { Id = 3, Name = "Enterprise", Code = "ENTERPRISE" });
        _tenantRepoMock.Setup(r => r.GetPlanFeaturesAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync([newlyAvailableFeatureId]);
        // Nothing currently enabled - the tenant never opted into the feature their old plan
        // didn't even offer.
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 3), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tenantRepoMock.Verify(
            r => r.BulkUpdateFeaturesAsync(It.IsAny<Guid>(), It.IsAny<Dictionary<Guid, bool>>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyOnThisPlan_ReturnsFailure()
    {
        var tenant = CreateTenantWithSubscription(currentPlanId: 2);
        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionPlan { Id = 2, Name = "Professional", Code = "PRO" });

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 2), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ReturnsFailure404()
    {
        var tenant = CreateTenantWithSubscription(currentPlanId: 1);
        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((SubscriptionPlan?)null);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 99), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
