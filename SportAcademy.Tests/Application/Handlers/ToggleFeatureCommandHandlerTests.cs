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
// tenant (same value, Lock flipped false).
public class ToggleFeatureCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ToggleFeatureCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid FeatureId = Guid.NewGuid();

    public ToggleFeatureCommandHandlerTests()
    {
        _handler = new ToggleFeatureCommandHandler(_tenantRepoMock.Object, _unitOfWorkMock.Object);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "Test", Slug = "test" });
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
}
