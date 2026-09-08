using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

// The SuperAdmin must be able to decide, on every toggle, whether the change also locks the
// feature against the tenant's own self-service toggle (Lock=true) or just nudges the current
// value while leaving (or restoring) the tenant's own control (Lock=false) - it must never be an
// unconditional force, and it must also support releasing an already-locked feature back to the
// tenant (same value, Lock flipped false). Also verifies FeatureDependencyPolicy's cascade rules:
// enabling a feature auto-enables its unmet prerequisites, disabling auto-disables dependents, and
// a SuperAdmin-locked conflict blocks the whole operation instead of being silently overridden.
public class ToggleFeatureCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ToggleFeatureCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid FeatureId = Guid.NewGuid();

    // "notifications" has no edges in FeatureDependencies - a neutral choice for tests that
    // aren't exercising the cascade logic itself.
    private static readonly Feature NeutralFeature = new()
    {
        Id = FeatureId,
        Name = "notifications",
        DisplayName = "Notifications",
        CreatedAt = DateTime.UtcNow,
    };

    public ToggleFeatureCommandHandlerTests()
    {
        _handler = new ToggleFeatureCommandHandler(_tenantRepoMock.Object, _unitOfWorkMock.Object);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "Test", Slug = "test" });
        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Feature> { NeutralFeature });
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantFeature>());
    }

    [Fact]
    public async Task Handle_NewFeatureWithLockTrue_CreatesItLocked()
    {
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, FeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantFeature?)null);

        TenantFeature? added = null;
        _tenantRepoMock
            .Setup(r => r.AddTenantFeatureAsync(It.IsAny<TenantFeature>(), It.IsAny<CancellationToken>()))
            .Callback<TenantFeature, CancellationToken>((f, _) => added = f)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, FeatureId, true, Lock: true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added!.IsEnabled.Should().BeTrue();
        added.LockedBySuperAdmin.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NewFeatureWithLockFalse_CreatesItUnlocked()
    {
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, FeatureId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantFeature?)null);

        TenantFeature? added = null;
        _tenantRepoMock
            .Setup(r => r.AddTenantFeatureAsync(It.IsAny<TenantFeature>(), It.IsAny<CancellationToken>()))
            .Callback<TenantFeature, CancellationToken>((f, _) => added = f)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, FeatureId, true, Lock: false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added!.LockedBySuperAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExistingUnlockedFeature_ToggleWithLockFalse_ChangesValueWithoutLocking()
    {
        var feature = new TenantFeature { TenantId = TenantId, FeatureId = FeatureId, IsEnabled = false, LockedBySuperAdmin = false };
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, FeatureId, It.IsAny<CancellationToken>())).ReturnsAsync(feature);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, FeatureId, true, Lock: false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        feature.IsEnabled.Should().BeTrue();
        feature.LockedBySuperAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExistingLockedFeature_SameValueButLockFalse_UnlocksIt()
    {
        // Releasing a previously-forced feature back to tenant self-service - value unchanged,
        // only the lock flips. Must not be rejected as a no-op.
        var feature = new TenantFeature { TenantId = TenantId, FeatureId = FeatureId, IsEnabled = true, LockedBySuperAdmin = true };
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, FeatureId, It.IsAny<CancellationToken>())).ReturnsAsync(feature);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, FeatureId, true, Lock: false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        feature.IsEnabled.Should().BeTrue();
        feature.LockedBySuperAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SameValueAndSameLock_ReturnsFailure()
    {
        var feature = new TenantFeature { TenantId = TenantId, FeatureId = FeatureId, IsEnabled = true, LockedBySuperAdmin = true };
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, FeatureId, It.IsAny<CancellationToken>())).ReturnsAsync(feature);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, FeatureId, true, Lock: true), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_EnablingDependent_CascadesUnmetPrerequisiteOn()
    {
        // coach-management requires branch-management (see FeatureDependencies).
        var coachId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var coachFeature = new Feature { Id = coachId, Name = "coach-management", DisplayName = "Coaches", CreatedAt = DateTime.UtcNow };
        var branchFeature = new Feature { Id = branchId, Name = "branch-management", DisplayName = "Branches", CreatedAt = DateTime.UtcNow };

        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Feature> { coachFeature, branchFeature });

        var branchTenantFeature = new TenantFeature { TenantId = TenantId, FeatureId = branchId, IsEnabled = false, LockedBySuperAdmin = false };
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantFeature> { branchTenantFeature });
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, coachId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantFeature?)null);
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branchTenantFeature);

        TenantFeature? added = null;
        _tenantRepoMock
            .Setup(r => r.AddTenantFeatureAsync(It.IsAny<TenantFeature>(), It.IsAny<CancellationToken>()))
            .Callback<TenantFeature, CancellationToken>((f, _) => added = f)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, coachId, true, Lock: false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        added!.FeatureId.Should().Be(coachId);
        added.IsEnabled.Should().BeTrue();
        branchTenantFeature.IsEnabled.Should().BeTrue();
        branchTenantFeature.EnabledBy.Should().Be("SuperAdmin (cascade)");
    }

    [Fact]
    public async Task Handle_DisablingProtectedFeature_WithoutConfirmation_ReturnsWarningAndLeavesItEnabled()
    {
        var protectedId = Guid.NewGuid();
        var protectedFeature = new Feature { Id = protectedId, Name = "user-management", DisplayName = "User Management", CreatedAt = DateTime.UtcNow };

        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Feature> { protectedFeature });

        var tenantFeature = new TenantFeature { TenantId = TenantId, FeatureId = protectedId, IsEnabled = true, LockedBySuperAdmin = false };
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, protectedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantFeature);

        var result = await _handler.Handle(
            new ToggleFeatureCommand(TenantId, protectedId, false, Lock: false, ConfirmProtectedDisable: false), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.Errors.Should().ContainKey("code").WhoseValue.Should().Contain("PROTECTED_FEATURE_CONFIRM_REQUIRED");
        tenantFeature.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_DisablingProtectedFeature_WithConfirmation_Succeeds()
    {
        var protectedId = Guid.NewGuid();
        var protectedFeature = new Feature { Id = protectedId, Name = "user-management", DisplayName = "User Management", CreatedAt = DateTime.UtcNow };

        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Feature> { protectedFeature });

        var tenantFeature = new TenantFeature { TenantId = TenantId, FeatureId = protectedId, IsEnabled = true, LockedBySuperAdmin = false };
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, protectedId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenantFeature);

        var result = await _handler.Handle(
            new ToggleFeatureCommand(TenantId, protectedId, false, Lock: false, ConfirmProtectedDisable: true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenantFeature.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DisablingPrerequisite_BlockedByLockedDependent()
    {
        // branch-management is required by coach-management, group-management and
        // pricing-management. If any dependent is currently enabled AND locked, disabling the
        // prerequisite must be rejected instead of silently overriding that lock.
        var coachId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var coachFeature = new Feature { Id = coachId, Name = "coach-management", DisplayName = "Coaches", CreatedAt = DateTime.UtcNow };
        var branchFeature = new Feature { Id = branchId, Name = "branch-management", DisplayName = "Branches", CreatedAt = DateTime.UtcNow };

        _tenantRepoMock.Setup(r => r.GetAllFeaturesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Feature> { coachFeature, branchFeature });

        var branchTenantFeature = new TenantFeature { TenantId = TenantId, FeatureId = branchId, IsEnabled = true, LockedBySuperAdmin = false };
        var coachTenantFeature = new TenantFeature { TenantId = TenantId, FeatureId = coachId, IsEnabled = true, LockedBySuperAdmin = true };
        _tenantRepoMock.Setup(r => r.GetTenantFeaturesAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TenantFeature> { branchTenantFeature, coachTenantFeature });
        _tenantRepoMock.Setup(r => r.GetTenantFeatureAsync(TenantId, branchId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(branchTenantFeature);

        var result = await _handler.Handle(new ToggleFeatureCommand(TenantId, branchId, false, Lock: false), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        branchTenantFeature.IsEnabled.Should().BeTrue();
        coachTenantFeature.IsEnabled.Should().BeTrue();
    }
}
