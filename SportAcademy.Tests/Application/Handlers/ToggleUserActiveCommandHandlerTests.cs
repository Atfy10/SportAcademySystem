using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class ToggleUserActiveCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IEffectiveLimitService> _limitServiceMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ToggleUserActiveCommandHandler _handler;

    public ToggleUserActiveCommandHandlerTests()
    {
        _userContextMock.Setup(c => c.TenantId).Returns(TenantId);
        _userContextMock.Setup(c => c.UserId).Returns((Guid?)null);

        _limitServiceMock
            .Setup(s => s.GetAsync(TenantId, LimitedResources.Users, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveLimit(LimitedResources.Users, null, LimitSource.Unlimited, 0, null));

        _handler = new ToggleUserActiveCommandHandler(
            _userRepoMock.Object, _userContextMock.Object, _limitServiceMock.Object, _publisherMock.Object);
    }

    private static AppUser CreateUser(Guid id, bool isBanned) => new()
    {
        Id = id,
        UserName = "test-user",
        Email = "test@example.com",
        IsBanned = isBanned,
    };

    [Fact]
    public async Task Handle_BanningActiveUser_Succeeds_WithoutCheckingLimit()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(userId, isBanned: false));

        var result = await _handler.Handle(new ToggleUserActiveCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeFalse(); // "is active" flips to false
        _limitServiceMock.Verify(
            s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never,
            "banning never consumes a seat, so it must never be checked against the cap");
    }

    [Fact]
    public async Task Handle_UnbanningUser_WithHeadroom_Succeeds()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(userId, isBanned: true));

        var result = await _handler.Handle(new ToggleUserActiveCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UnbanningUser_AtCap_ReturnsLimitExceeded()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateUser(userId, isBanned: true));
        _limitServiceMock
            .Setup(s => s.GetAsync(TenantId, LimitedResources.Users, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveLimit(LimitedResources.Users, 5, LimitSource.Plan, 5, null));

        var result = await _handler.Handle(new ToggleUserActiveCommand(userId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Errors!["code"].Should().Contain("LIMIT_EXCEEDED");
        _userRepoMock.Verify(r => r.UpdateAsync(It.IsAny<AppUser>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsIdNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((AppUser?)null);

        var act = () => _handler.Handle(new ToggleUserActiveCommand(userId), CancellationToken.None);
        await act.Should().ThrowAsync<IdNotFoundException>();
    }
}
