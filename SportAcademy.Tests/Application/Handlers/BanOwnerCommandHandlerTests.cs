using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.BanOwner;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Tests.Application.Handlers;

public class BanOwnerCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ITenantIdProvider> _tenantIdProviderMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly BanOwnerCommandHandler _handler;

    public BanOwnerCommandHandlerTests()
    {
        _tenantIdProviderMock.Setup(p => p.Impersonate(It.IsAny<Guid>())).Returns(Mock.Of<IDisposable>());
        _handler = new BanOwnerCommandHandler(
            _userRepoMock.Object, _tenantIdProviderMock.Object, _refreshTokenRepoMock.Object);
    }

    private static AppUser CreateOwner(Guid tenantId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        UserName = "owner",
        Email = "owner@test.com",
        Tenant = new Tenant { Id = tenantId, Name = "Test Academy", Slug = "test-academy" },
    };

    [Fact]
    public async Task Handle_OwnerNotFound_ReturnsFailure404()
    {
        _userRepoMock.Setup(r => r.GetOwnerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AppUser?)null);

        var result = await _handler.Handle(new BanOwnerCommand(Guid.NewGuid(), true), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_Ban_SetsIsBannedAndRevokesRefreshTokens()
    {
        // F-02: banning must kill the owner's live session immediately, not leave it to fail
        // only the next time a held refresh token is actually presented.
        var owner = CreateOwner(Guid.NewGuid());
        _userRepoMock.Setup(r => r.GetOwnerByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);

        var result = await _handler.Handle(new BanOwnerCommand(owner.Id, true), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        owner.IsBanned.Should().BeTrue();
        _userRepoMock.Verify(r => r.UpdateAsync(owner, It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepoMock.Verify(r => r.RevokeAllUserTokensAsync(owner.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Unban_DoesNotRevokeAnyRefreshTokens()
    {
        // Un-banning shouldn't touch sessions at all - the owner just logs back in normally.
        var owner = CreateOwner(Guid.NewGuid());
        owner.IsBanned = true;
        _userRepoMock.Setup(r => r.GetOwnerByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);

        var result = await _handler.Handle(new BanOwnerCommand(owner.Id, false), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        owner.IsBanned.Should().BeFalse();
        _refreshTokenRepoMock.Verify(
            r => r.RevokeAllUserTokensAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Ban_ImpersonatesOwnersTenantForTheWrite()
    {
        var owner = CreateOwner(Guid.NewGuid());
        _userRepoMock.Setup(r => r.GetOwnerByIdAsync(owner.Id, It.IsAny<CancellationToken>())).ReturnsAsync(owner);

        await _handler.Handle(new BanOwnerCommand(owner.Id, true), CancellationToken.None);

        _tenantIdProviderMock.Verify(p => p.Impersonate(owner.TenantId), Times.Once);
    }
}
