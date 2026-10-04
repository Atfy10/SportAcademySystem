using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Implementations;

namespace SportAcademy.Tests.Infrastructure.Implementations;

public class SessionRevocationServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock = new(
        Mock.Of<IUserStore<AppUser>>(), null!, null!, null!, null!, null!, null!, null!, null!);
    private readonly Mock<ITenantIdProvider> _tenantIdProviderMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<ISecurityStampCacheInvalidator> _stampCacheMock = new();
    private readonly Mock<IPermissionCacheInvalidator> _permissionCacheMock = new();
    private readonly Mock<IRealtimeService> _realtimeMock = new();
    private readonly SessionRevocationService _service;

    private readonly AppUser _user = new()
    {
        Id = Guid.NewGuid(),
        TenantId = Guid.NewGuid(),
        UserName = "user",
        Email = "user@test.com",
    };

    public SessionRevocationServiceTests()
    {
        _tenantIdProviderMock.Setup(p => p.Impersonate(It.IsAny<Guid>())).Returns(Mock.Of<IDisposable>());
        _userManagerMock.Setup(m => m.UpdateSecurityStampAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);

        _service = new SessionRevocationService(
            _userManagerMock.Object,
            _tenantIdProviderMock.Object,
            _refreshTokenRepoMock.Object,
            _stampCacheMock.Object,
            _permissionCacheMock.Object,
            _realtimeMock.Object);
    }

    [Fact]
    public async Task RevokeAllSessions_RotatesStamp_RevokesRefreshTokens_InvalidatesCaches_AndPushes()
    {
        await _service.RevokeAllSessionsAsync(_user, SessionRevocationReasons.RolesChanged);

        _userManagerMock.Verify(m => m.UpdateSecurityStampAsync(_user), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.RevokeAllUserTokensAsync(_user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _stampCacheMock.Verify(c => c.Invalidate(_user.Id), Times.Once);
        _permissionCacheMock.Verify(c => c.Invalidate(_user.Id), Times.Once);
        _realtimeMock.Verify(r => r.NotifySessionRevokedAsync(_user.Id, SessionRevocationReasons.RolesChanged), Times.Once);
    }

    [Fact]
    public async Task RevokeAllSessions_WritesTheStampUnderTheUsersOwnTenant()
    {
        // The anonymous reset-password link has no ambient tenant, and BanOwner runs as the
        // System tenant - TenantSaveChangesInterceptor would refuse the AppUser write in both.
        await _service.RevokeAllSessionsAsync(_user, SessionRevocationReasons.PasswordChanged);

        _tenantIdProviderMock.Verify(p => p.Impersonate(_user.TenantId), Times.Once);
    }

    [Fact]
    public async Task RevokeAllSessions_FailedStampRotation_Throws_AndLeavesEverythingElseAlone()
    {
        _userManagerMock.Setup(m => m.UpdateSecurityStampAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "boom" }));

        var act = () => _service.RevokeAllSessionsAsync(_user, SessionRevocationReasons.PasswordChanged);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _realtimeMock.Verify(r => r.NotifySessionRevokedAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }
}
