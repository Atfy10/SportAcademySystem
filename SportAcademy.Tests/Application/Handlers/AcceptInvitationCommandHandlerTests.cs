using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.AcceptInvitation;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Entities.Tenants;
using RefreshTokenEntity = SportAcademy.Domain.Entities.RefreshToken;

namespace SportAcademy.Tests.Application.Handlers;

public class AcceptInvitationCommandHandlerTests
{
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IInvitationRepository> _invitationRepoMock = new();
    private readonly Mock<IBaseRepository<Tenant, Guid>> _tenantRepoMock = new();
    private readonly Mock<IBaseRepository<RefreshTokenEntity, int>> _refreshTokenRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly Mock<IUserPermissionOverrideRepository> _userPermissionOverrideRepoMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AcceptInvitationCommandHandler _handler;

    public AcceptInvitationCommandHandlerTests()
    {
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        _handler = new AcceptInvitationCommandHandler(
            _tokenServiceMock.Object,
            _invitationRepoMock.Object,
            _tenantRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _unitOfWorkMock.Object,
            _userManagerMock.Object,
            _jwtTokenServiceMock.Object,
            _userPermissionOverrideRepoMock.Object,
            _mediatorMock.Object);
    }

    private static Invitation CreatePendingInvitation(Guid tenantId, string email = "owner@test.com")
    {
        var invitation = new Invitation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Email = email,
            TokenHash = "hashed-token",
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Purpose = InvitationPurpose.OwnerSetup,
            InvitedByUserId = Guid.NewGuid()
        };
        return invitation;
    }

    private static Tenant CreateTenant(Guid id) => new()
    {
        Id = id,
        Name = "Test Academy",
        Slug = "test-academy",
        Status = TenantStatus.PendingSetup
    };

    private static AcceptInvitationCommand CreateValidCommand(string slug = "test-academy") =>
        new("raw-token", "StrongPass1!", slug);

    [Fact]
    public async Task Handle_ValidInvitation_ReturnsAuthResponse()
    {
        var tenantId = Guid.NewGuid();
        var invitation = CreatePendingInvitation(tenantId);
        var tenant = CreateTenant(tenantId);
        var command = CreateValidCommand();

        _tokenServiceMock
            .Setup(s => s.HashToken("raw-token"))
            .Returns("hashed-token");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<AppUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), "Owner"))
            .ReturnsAsync(IdentityResult.Success);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("plain-refresh-token");

        _jwtTokenServiceMock
            .Setup(j => j.HashToken("plain-refresh-token"))
            .Returns("hashed-refresh-token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateJwtToken(It.IsAny<AppUser>(), "Owner"))
            .ReturnsAsync("jwt-access-token");

        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("jwt-access-token");
        result.Data.RefreshToken.Should().Be("plain-refresh-token");

        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.UsedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExpiredInvitation_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        var invitation = CreatePendingInvitation(tenantId);
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        var command = CreateValidCommand();

        _tokenServiceMock
            .Setup(s => s.HashToken("raw-token"))
            .Returns("hashed-token");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        invitation.Status.Should().Be(InvitationStatus.Expired);
    }

    [Fact]
    public async Task Handle_InvitationNotFound_ReturnsNotFound()
    {
        _tokenServiceMock
            .Setup(s => s.HashToken("raw-token"))
            .Returns("hashed-token");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invitation?)null);

        var result = await _handler.Handle(CreateValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_TenantNotPendingSetup_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        var invitation = CreatePendingInvitation(tenantId);
        var tenant = CreateTenant(tenantId);
        tenant.Status = TenantStatus.Active;
        var command = CreateValidCommand();

        _tokenServiceMock
            .Setup(s => s.HashToken("raw-token"))
            .Returns("hashed-token");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_UserCreationFails_RollsBack()
    {
        var tenantId = Guid.NewGuid();
        var invitation = CreatePendingInvitation(tenantId);
        var tenant = CreateTenant(tenantId);
        var command = CreateValidCommand();

        _tokenServiceMock
            .Setup(s => s.HashToken("raw-token"))
            .Returns("hashed-token");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        _tenantRepoMock
            .Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _unitOfWorkMock
            .Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<AppUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Username already taken." }));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
