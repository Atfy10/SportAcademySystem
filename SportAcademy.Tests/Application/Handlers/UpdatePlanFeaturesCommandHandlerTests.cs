using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.UpdatePlanFeatures;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

// Editing a plan's own feature set must retroactively reconcile every tenant currently
// subscribed to it (confirmed product decision) using the exact same symmetric rule
// ChangeTenantPlanCommandHandlerTests already locks in for a per-tenant plan change.
public class UpdatePlanFeaturesCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IBaseRepository<SubscriptionPlan, int>> _planRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdatePlanFeaturesCommandHandler _handler;

    private const int PlanId = 2;

    public UpdatePlanFeaturesCommandHandlerTests()
    {
        _handler = new UpdatePlanFeaturesCommandHandler(_tenantRepoMock.Object, _planRepoMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_PlanNotFound_ReturnsFailure404()
    {
        _planRepoMock.Setup(r => r.GetByIdAsync(PlanId, It.IsAny<CancellationToken>())).ReturnsAsync((SubscriptionPlan?)null);

        var result = await _handler.Handle(new UpdatePlanFeaturesCommand(PlanId, []), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_UnknownFeatureId_ReturnsFailure400()
    {
        _planRepoMock.Setup(r => r.GetByIdAsync(PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionPlan { Id = PlanId, Name = "Professional", Code = "PRO" });
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Feature { Id = Guid.NewGuid(), Name = "known", DisplayName = "Known" }]);

        var result = await _handler.Handle(new UpdatePlanFeaturesCommand(PlanId, [Guid.NewGuid()]), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_Success_ReplacesPlanFeaturesAndReconcilesSubscribedTenants()
    {
        var keptFeatureId = Guid.NewGuid();
        var newlyAddedFeatureId = Guid.NewGuid();
        var removedFeatureId = Guid.NewGuid();
        var tenantOnPlan = Guid.NewGuid();
        var lockedFeatureId = Guid.NewGuid();

        _planRepoMock.Setup(r => r.GetByIdAsync(PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionPlan { Id = PlanId, Name = "Professional", Code = "PRO" });
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new Feature { Id = keptFeatureId, Name = "kept", DisplayName = "Kept" },
                new Feature { Id = newlyAddedFeatureId, Name = "added", DisplayName = "Added" },
                new Feature { Id = lockedFeatureId, Name = "locked", DisplayName = "Locked" },
            ]);
        _tenantRepoMock.Setup(r => r.GetPlanFeaturesAsync(PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([keptFeatureId, removedFeatureId]);
        _tenantRepoMock.Setup(r => r.GetTenantIdsSubscribedToPlanAsync(PlanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([tenantOnPlan]);
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(tenantOnPlan, It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new TenantFeature { TenantId = tenantOnPlan, FeatureId = keptFeatureId, IsEnabled = true },
                new TenantFeature { TenantId = tenantOnPlan, FeatureId = removedFeatureId, IsEnabled = true },
                new TenantFeature { TenantId = tenantOnPlan, FeatureId = lockedFeatureId, IsEnabled = false, LockedBySuperAdmin = true },
            ]);

        Dictionary<Guid, bool>? capturedUpdates = null;
        _tenantRepoMock
            .Setup(r => r.BulkUpdateFeaturesAsync(tenantOnPlan, It.IsAny<Dictionary<Guid, bool>>(), "PlanFeaturesUpdated", It.IsAny<CancellationToken>()))
            .Callback<Guid, Dictionary<Guid, bool>, string, CancellationToken>((_, updates, _, _) => capturedUpdates = updates)
            .Returns(Task.CompletedTask);

        var newFeatureIds = new List<Guid> { keptFeatureId, newlyAddedFeatureId, lockedFeatureId };
        var result = await _handler.Handle(new UpdatePlanFeaturesCommand(PlanId, newFeatureIds), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _tenantRepoMock.Verify(r => r.ReplacePlanFeaturesAsync(PlanId, newFeatureIds, It.IsAny<CancellationToken>()), Times.Once);

        capturedUpdates.Should().NotBeNull();
        capturedUpdates!.Should().ContainKey(newlyAddedFeatureId).WhoseValue.Should().BeTrue();
        capturedUpdates.Should().ContainKey(removedFeatureId).WhoseValue.Should().BeFalse();
        capturedUpdates.Should().NotContainKey(keptFeatureId);
        capturedUpdates.Should().NotContainKey(lockedFeatureId);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
