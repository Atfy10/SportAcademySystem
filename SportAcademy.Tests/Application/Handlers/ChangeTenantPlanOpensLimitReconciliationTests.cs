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
// that puts the tenant over a new, lower limit must open a reconciliation and say so, and one
// that doesn't must leave the tenant alone and say the plain thing.
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
    public async Task Handle_DowngradePutsTenantOverLimit_OpensReconciliation_AndSaysSo()
    {
        _limitReconciliationServiceMock
            .Setup(s => s.EvaluateAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("forced selection");
        _limitReconciliationServiceMock.Verify(s => s.EvaluateAsync(TenantId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once,
            "the plan change and the reconciliation lock must commit in the same transaction");
    }

    [Fact]
    public async Task Handle_DowngradeStaysWithinLimits_DoesNotMentionReconciliation()
    {
        _limitReconciliationServiceMock
            .Setup(s => s.EvaluateAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new ChangeTenantPlanCommand(TenantId, 1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().NotContain("forced selection");
    }
}
