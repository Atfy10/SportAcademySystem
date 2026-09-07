using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class ArchiveTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantStatusCacheInvalidator> _tenantStatusCacheMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IRealtimeService> _realtimeServiceMock = new();
    private readonly ArchiveTenantCommandHandler _handler;

    public ArchiveTenantCommandHandlerTests()
    {
        _userRepoMock
            .Setup(r => r.GetUserIdsByTenantIgnoringTenantAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _handler = new ArchiveTenantCommandHandler(
            _tenantRepoMock.Object,
            _unitOfWorkMock.Object,
            _tenantStatusCacheMock.Object,
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _realtimeServiceMock.Object);
    }

    private static Tenant CreateTenant(Guid id, TenantStatus status) => new()
    {
        Id = id,
        Name = "Test Academy",
        Slug = "test-academy",
        Status = status,
    };

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure404()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_AlreadyArchived_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Archived);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveTenant_IsNoLongerArchivableDirectly()
    {
        // Before F-03/F-04 this endpoint allowed Active -> Archived even though
        // ChangeTenantStatusCommandHandler forbade it. Now both agree: a live tenant must be
        // suspended or deactivated first.
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        tenant.Status.Should().Be(TenantStatus.Active);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_PendingSetupTenant_IsRejected()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.PendingSetup);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Inactive)]
    public async Task Handle_SuspendedOrInactiveTenant_ArchivesSuccessfully(TenantStatus status)
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, status);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Archived);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _tenantStatusCacheMock.Verify(c => c.Invalidate(tenantId), Times.Once);
    }

    [Fact]
    public async Task Handle_Archives_RevokesEveryAffectedUsersRefreshTokens()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Suspended);
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _userRepoMock.Setup(r => r.GetUserIdsByTenantIgnoringTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userIds);

        await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        _refreshTokenRepoMock.Verify(r => r.RevokeAllTokensForUsersAsync(userIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Archives_NotifiesConnectedClientsToDisconnect()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Suspended);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        await _handler.Handle(new ArchiveTenantCommand(tenantId, "Test reason"), CancellationToken.None);

        _realtimeServiceMock.Verify(
            r => r.NotifyTenantStatusChangedAsync(tenantId, TenantStatus.Archived.ToString()), Times.Once);
    }
}
