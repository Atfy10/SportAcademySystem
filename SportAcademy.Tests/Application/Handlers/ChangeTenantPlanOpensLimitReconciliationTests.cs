using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

// Separate from ChangeTenantPlanCommandHandlerTests (which covers feature reconciliation and
// stubs the limit-reconciliation trigger out) - this covers the trigger itself: a plan change
// that puts the tenant over a new limit must open a reconciliation and say so, one that doesn't
// must leave the tenant alone and say the plain thing, and one that resolves or refreshes an
// already-open reconciliation must report that instead. Deliberately not framed around
// "upgrade"/"downgrade" - plan tiers aren't ranked, so the handler (and ReconcileAsync) treat
// every plan change identically regardless of direction.
public class ChangeTenantPlanOpensLimitReconciliationTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IBaseRepository<SubscriptionPlan, int>> _planRepoMock = new();
    private readonly Mock<ILimitReconciliationService> _limitReconciliationServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ChangeTenantPlanCommandHandler _handler;

    public ChangeTenantPlanOpensLimitReconciliationTests()
    {
        _handler = new ChangeTenantPlanCommandHandler(
            _tenantRepoMock.Object, _planRepoMock.Object, _limitReconciliationServiceMock.Object, _unitOfWorkMock.Object);

        var tenant = new Tenant
        {
            Id = TenantId,
            Name = "Test",
            Slug = "test",
            Subscription = new TenantSubscription { TenantId = TenantId, SubscriptionPlanId = 2 },
        };
        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _planRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionPlan { Id = 1, Name = "Basic", Code = "BASIC" });
        _tenantRepoMock.Setup(r => r.GetPlanFeaturesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new List<Guid>());
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(new List<TenantFeature>());
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Feature>());
    }

    [Fact]
    public async Task Handle_PlanChangePutsTenantOverLimit_OpensReconciliation_AndSaysSo()
    {
        _limitReconciliationServiceMock
            .Setup(s => s.ReconcileAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LimitReconciliationOutcome.Opened);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("forced selection");
        _limitReconciliationServiceMock.Verify(s => s.ReconcileAsync(TenantId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once,
            "the plan change and the reconciliation lock must commit in the same transaction");
    }

    [Fact]
    public async Task Handle_PlanChangeStaysWithinLimits_DoesNotMentionReconciliation()
    {
        _limitReconciliationServiceMock
            .Setup(s => s.ReconcileAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LimitReconciliationOutcome.NoChange);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().NotContain("forced selection");
    }

    // The two tests below prove the handler never branches on "direction" itself - it always
    // calls ReconcileAsync exactly once and just reports whatever outcome comes back. Plan tiers
    // aren't ranked (new plans can be added with no fixed hierarchy), so a tenant already
    // PendingLimitSelection is handled identically here whether the plan swap happens to raise or
    // lower its effective limits - only ReconcileAsync's own answer matters.

    [Fact]
    public async Task Handle_PlanChangeResolvesExistingLock_WhenNoLongerOverAnyLimit()
    {
        var lockedTenant = new Tenant
        {
            Id = TenantId,
            Name = "Test",
            Slug = "test",
            Status = SportAcademy.Domain.Enums.TenantStatus.PendingLimitSelection,
            Subscription = new TenantSubscription { TenantId = TenantId, SubscriptionPlanId = 2 },
        };
        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(lockedTenant);

        _limitReconciliationServiceMock
            .Setup(s => s.ReconcileAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LimitReconciliationOutcome.Resolved);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("within its limits again and is active");
        _limitReconciliationServiceMock.Verify(s => s.ReconcileAsync(TenantId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PlanChangeLeavesLockInPlace_WhenStillOverALimit()
    {
        var lockedTenant = new Tenant
        {
            Id = TenantId,
            Name = "Test",
            Slug = "test",
            Status = SportAcademy.Domain.Enums.TenantStatus.PendingLimitSelection,
            Subscription = new TenantSubscription { TenantId = TenantId, SubscriptionPlanId = 2 },
        };
        _tenantRepoMock.Setup(r => r.GetDetailByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(lockedTenant);

        _limitReconciliationServiceMock
            .Setup(s => s.ReconcileAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LimitReconciliationOutcome.StillPending);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("remains over one or more limits");
    }
}
