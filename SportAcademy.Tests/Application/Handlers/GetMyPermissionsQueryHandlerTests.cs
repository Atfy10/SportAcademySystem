using FluentAssertions;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.AuthQueries.GetMyPermissions;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class GetMyPermissionsQueryHandlerTests
{
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPermissionResolver> _permissionResolverMock = new();
    private readonly Mock<ITenantStatusCache> _tenantStatusCacheMock = new();
    private readonly GetMyPermissionsQueryHandler _handler;

    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _tenantId = Guid.NewGuid();

    public GetMyPermissionsQueryHandlerTests()
    {
        _userContextMock.Setup(c => c.UserId).Returns(_userId);
        _userContextMock.Setup(c => c.TenantId).Returns(_tenantId);
        _handler = new GetMyPermissionsQueryHandler(
            _userContextMock.Object, _userRepoMock.Object, _permissionResolverMock.Object, _tenantStatusCacheMock.Object);
    }

    private static AppUser CreateUser(Guid id, bool isBanned = false) => new()
    {
        Id = id,
        UserName = "test-user",
        Email = "test-user@test.com",
        IsBanned = isBanned,
    };

    [Fact]
    public async Task Handle_ActiveUserActiveTenant_ReturnsIsActiveTrue()
    {
        var user = CreateUser(_userId);
        _userRepoMock.Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetUserRoleAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(["Owner"]);
        _permissionResolverMock.Setup(p => p.GetEffectivePermissionsAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string> { "trainee.register" });
        _tenantStatusCacheMock.Setup(c => c.GetStatusAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.Active);

        var result = await _handler.Handle(new GetMyPermissionsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.IsActive.Should().BeTrue();
        result.Data.TenantStatus.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public async Task Handle_BannedUser_ReturnsIsActiveFalse()
    {
        // AuthContext's polling loop treats isActive:false as a signal to log the session out
        // (F-02) - a banned user must see this on the very next poll.
        var user = CreateUser(_userId, isBanned: true);
        _userRepoMock.Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetUserRoleAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(["Owner"]);
        _permissionResolverMock.Setup(p => p.GetEffectivePermissionsAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());
        _tenantStatusCacheMock.Setup(c => c.GetStatusAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TenantStatus.Active);

        var result = await _handler.Handle(new GetMyPermissionsQuery(), CancellationToken.None);

        result.Data!.IsActive.Should().BeFalse();
    }

    [Theory]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Inactive)]
    [InlineData(TenantStatus.Archived)]
    public async Task Handle_NonActiveTenant_ReturnsThatStatus(TenantStatus status)
    {
        var user = CreateUser(_userId);
        _userRepoMock.Setup(r => r.GetByIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.GetUserRoleAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(["Owner"]);
        _permissionResolverMock.Setup(p => p.GetEffectivePermissionsAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HashSet<string>());
        _tenantStatusCacheMock.Setup(c => c.GetStatusAsync(_tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(status);

        var result = await _handler.Handle(new GetMyPermissionsQuery(), CancellationToken.None);

        result.Data!.TenantStatus.Should().Be(status);
    }
}
