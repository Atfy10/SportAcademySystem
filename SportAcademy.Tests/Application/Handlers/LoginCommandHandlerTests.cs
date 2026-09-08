using AutoMapper;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.Login;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<ITenantIdProvider> _tenantIdProviderMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(
            _jwtTokenServiceMock.Object,
            _userRepoMock.Object,
            _roleRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _mapperMock.Object,
            _tenantIdProviderMock.Object,
            _tenantRepoMock.Object,
            _mediatorMock.Object);
    }

    private static Tenant CreateTenant(TenantStatus status, string code = "TEST", DateTime? firstLoginAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Academy",
        DisplayName = "Test Academy",
        Slug = "test-academy",
        Code = code,
        Status = status,
        FirstLoginAt = firstLoginAt,
    };

    private static AppUser CreateUser(Guid tenantId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenantId,
        UserName = "owner",
        Email = "owner@test.com",
        IsBanned = false,
    };

    [Theory]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Inactive)]
    [InlineData(TenantStatus.Archived)]
    [InlineData(TenantStatus.PendingSetup)]
    public async Task Handle_NonActiveTenant_ThrowsUserLoginExceptionWithoutCheckingUser(TenantStatus status)
    {
        // A suspended/archived/deactivated/not-yet-set-up tenant must refuse a brand new login
        // outright (F-01) - and must never even query the user, which would otherwise leak
        // whether the account exists via timing/behavior differences.
        var tenant = CreateTenant(status);
        _tenantRepoMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        await Assert.ThrowsAsync<UserLoginException>(() =>
            _handler.Handle(new LoginCommand("owner", "password", "test-academy"), CancellationToken.None));

        _userRepoMock.Verify(
            r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveTenantBannedUser_ThrowsUserLoginException()
    {
        var tenant = CreateTenant(TenantStatus.Active);
        var user = CreateUser(tenant.Id);
        user.IsBanned = true;

        _tenantRepoMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await Assert.ThrowsAsync<UserLoginException>(() =>
            _handler.Handle(new LoginCommand("owner", "password", "test-academy"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ActiveTenantValidCredentials_Succeeds()
    {
        var tenant = CreateTenant(TenantStatus.Active);
        var user = CreateUser(tenant.Id);

        _tenantRepoMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.CheckPasswordAsync(user, It.IsAny<string>()))
            .ReturnsAsync(true);
        _roleRepoMock.Setup(r => r.GetRolesForUser(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Owner"]);
        _jwtTokenServiceMock.Setup(j => j.GenerateJwtToken(user, It.IsAny<string[]>()))
            .ReturnsAsync("access-token");
        _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hashed");

        var result = await _handler.Handle(
            new LoginCommand("owner", "password", "test-academy"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _tenantIdProviderMock.Verify(p => p.SetTenantId(tenant.Id), Times.Once);
    }

    private void SetUpSuccessfulCredentials(Tenant tenant, AppUser user)
    {
        _tenantRepoMock.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _userRepoMock.Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepoMock.Setup(r => r.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        _roleRepoMock.Setup(r => r.GetRolesForUser(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(["Owner"]);
        _jwtTokenServiceMock.Setup(j => j.GenerateJwtToken(user, It.IsAny<string[]>())).ReturnsAsync("access-token");
        _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _jwtTokenServiceMock.Setup(j => j.HashToken(It.IsAny<string>())).Returns("hashed");
    }

    [Fact]
    public async Task Handle_TenantsFirstLogin_SetsFirstLoginAtAndPublishesEvent()
    {
        var tenant = CreateTenant(TenantStatus.Active, firstLoginAt: null);
        var user = CreateUser(tenant.Id);
        SetUpSuccessfulCredentials(tenant, user);

        var result = await _handler.Handle(new LoginCommand("owner", "password", "test-academy"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(tenant.FirstLoginAt);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<FirstTenantLoginEvent>(e => e.TenantId == tenant.Id && e.UserId == user.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TenantAlreadyLoggedInBefore_DoesNotPublishEventAgain()
    {
        var tenant = CreateTenant(TenantStatus.Active, firstLoginAt: DateTime.UtcNow.AddDays(-30));
        var user = CreateUser(tenant.Id);
        SetUpSuccessfulCredentials(tenant, user);

        await _handler.Handle(new LoginCommand("owner", "password", "test-academy"), CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<FirstTenantLoginEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SystemTenantFirstLogin_DoesNotPublishEvent()
    {
        // A SuperAdmin's own first login has no other SuperAdmin to notify and isn't the "a
        // tenant started using the product" milestone this event exists to surface.
        var tenant = CreateTenant(TenantStatus.Active, code: Tenant.SystemTenantCode, firstLoginAt: null);
        var user = CreateUser(tenant.Id);
        SetUpSuccessfulCredentials(tenant, user);

        await _handler.Handle(new LoginCommand("owner", "password", "system"), CancellationToken.None);

        _mediatorMock.Verify(m => m.Publish(It.IsAny<FirstTenantLoginEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
