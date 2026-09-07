using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class ChangeTenantStatusCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITenantStatusCacheInvalidator> _tenantStatusCacheMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IRealtimeService> _realtimeServiceMock = new();
    private readonly ChangeTenantStatusCommandHandler _handler;

    public ChangeTenantStatusCommandHandlerTests()
    {
        _userRepoMock
            .Setup(r => r.GetUserIdsByTenantIgnoringTenantAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _handler = new ChangeTenantStatusCommandHandler(
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

        var result = await _handler.Handle(
            new ChangeTenantStatusCommand(tenantId, TenantStatus.Suspended), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_SameStatus_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ChangeTenantStatusCommand(tenantId, TenantStatus.Active), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveToArchivedDirectly_IsRejected()
    {
        // This is the transition that used to be permitted by DELETE and forbidden here (F-03).
        // The handler must still forbid it now that both routes share TenantStatusPolicy.
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ChangeTenantStatusCommand(tenantId, TenantStatus.Archived), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        tenant.Status.Should().Be(TenantStatus.Active);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _tenantStatusCacheMock.Verify(c => c.Invalidate(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ArchivedToSuspended_IsPermitted()
    {
        // The restore path added by F-04: an archived tenant can come back to Suspended (never
        // straight to Active).
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Archived);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ChangeTenantStatusCommand(tenantId, TenantStatus.Suspended), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Suspended);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidTransition_Succeeds()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(
            new ChangeTenantStatusCommand(tenantId, TenantStatus.Suspended), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Suspended);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidTransition_InvalidatesTenantStatusCache()
    {
        // The guard middleware would otherwise keep enforcing the pre-suspension status for up
        // to the cache's 5-minute sliding window (F-01's whole point is that this takes effect
        // on the very next request).
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        await _handler.Handle(new ChangeTenantStatusCommand(tenantId, TenantStatus.Suspended), CancellationToken.None);

        _tenantStatusCacheMock.Verify(c => c.Invalidate(tenantId), Times.Once);
    }

    [Theory]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Inactive)]
    public async Task Handle_TransitionAwayFromActive_RevokesEveryAffectedUsersRefreshTokens(TenantStatus newStatus)
    {
        // F-02: a suspended/deactivated tenant's users must not be able to keep a session alive
        // by refreshing - every live refresh token for the tenant is revoked in the same
        // request as the status change, not left to fail only the next time one is presented.
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _userRepoMock.Setup(r => r.GetUserIdsByTenantIgnoringTenantAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userIds);

        await _handler.Handle(new ChangeTenantStatusCommand(tenantId, newStatus), CancellationToken.None);

        _refreshTokenRepoMock.Verify(r => r.RevokeAllTokensForUsersAsync(userIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TransitionIntoActive_DoesNotRevokeAnyRefreshTokens()
    {
        // A reactivated tenant's users should simply be able to log back in normally - nothing
        // about coming back to Active should touch existing sessions.
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Suspended);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        await _handler.Handle(new ChangeTenantStatusCommand(tenantId, TenantStatus.Active), CancellationToken.None);

        _userRepoMock.Verify(
            r => r.GetUserIdsByTenantIgnoringTenantAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepoMock.Verify(
            r => r.RevokeAllTokensForUsersAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        _realtimeServiceMock.Verify(
            r => r.NotifyTenantStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Inactive)]
    public async Task Handle_TransitionAwayFromActive_NotifiesConnectedClientsToDisconnect(TenantStatus newStatus)
    {
        // An already-open SignalR connection keeps working after the status flips until it
        // disconnects on its own otherwise - this push is what makes suspend actually
        // immediate for a live session, not just for the next API call.
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        await _handler.Handle(new ChangeTenantStatusCommand(tenantId, newStatus), CancellationToken.None);

        _realtimeServiceMock.Verify(
            r => r.NotifyTenantStatusChangedAsync(tenantId, newStatus.ToString()), Times.Once);
    }
}
