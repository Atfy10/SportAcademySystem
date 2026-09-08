using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
    private readonly Mock<IUserBranchAccessRepository> _userBranchAccessRepoMock = new();
    private readonly Mock<IProfileRepository> _profileRepoMock = new();
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
            _userBranchAccessRepoMock.Object,
            _profileRepoMock.Object,
            _mediatorMock.Object,
            Mock.Of<ILogger<AcceptInvitationCommandHandler>>());
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
            InvitedByUserId = Guid.NewGuid(),
            // Every existing test in this file exercises the accept flow itself, not the
            // separate email-verification gate - defaulting to already-verified keeps them
            // focused on what they actually test. Handle_EmailNotVerified_ReturnsFailure below
            // covers the gate directly.
            IsEmailVerified = true,
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

        _profileRepoMock
            .Setup(r => r.AddAsyncWithoutSave(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile p, CancellationToken _) => p);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().Be("jwt-access-token");
        result.Data.RefreshToken.Should().Be("plain-refresh-token");

        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.UsedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Published after the commit, not from inside the try/catch - a failure in a subscriber
        // must never be mistaken for a mid-transaction failure (see UnitOfWork's rollback guard).
        _mediatorMock.Verify(
            m => m.Publish(It.IsAny<SportAcademy.Domain.Events.InvitationAcceptedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailNotVerified_ReturnsFailureWithoutCreatingAUser()
    {
        var tenantId = Guid.NewGuid();
        var invitation = CreatePendingInvitation(tenantId);
        invitation.IsEmailVerified = false;
        var tenant = CreateTenant(tenantId);
        var command = CreateValidCommand();

        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _userManagerMock.Verify(um => um.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()), Times.Never);
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

    [Fact]
    public async Task Handle_SubscriberThrowsAfterCommit_StillReturnsSuccessAndNeverRollsBack()
    {
        // The account and (for an Owner) tenant activation are already durably committed by the
        // time InvitationAcceptedEvent is published - a failure in a subscriber (e.g. the
        // "notify the inviter" side effect) must never be reported back as "onboarding failed"
        // when the tenant is actually already Active, nor attempt a rollback of a transaction
        // that no longer exists.
        var tenantId = Guid.NewGuid();
        var invitation = CreatePendingInvitation(tenantId);
        var tenant = CreateTenant(tenantId);
        var command = CreateValidCommand();

        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _userManagerMock
            .Setup(um => um.CreateAsync(It.IsAny<AppUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock
            .Setup(um => um.AddToRoleAsync(It.IsAny<AppUser>(), "Owner"))
            .ReturnsAsync(IdentityResult.Success);
        _jwtTokenServiceMock.Setup(j => j.GenerateRefreshToken()).Returns("plain-refresh-token");
        _jwtTokenServiceMock.Setup(j => j.HashToken("plain-refresh-token")).Returns("hashed-refresh-token");
        _jwtTokenServiceMock
            .Setup(j => j.GenerateJwtToken(It.IsAny<AppUser>(), "Owner"))
            .ReturnsAsync("jwt-access-token");
        _refreshTokenRepoMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _profileRepoMock
            .Setup(r => r.AddAsyncWithoutSave(It.IsAny<Profile>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Profile p, CancellationToken _) => p);
        _mediatorMock
            .Setup(m => m.Publish(It.IsAny<SportAcademy.Domain.Events.InvitationAcceptedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("subscriber blew up"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("jwt-access-token");
        tenant.Status.Should().Be(TenantStatus.Active);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
