using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.InvitationDtos;
using SportAcademy.Application.Mappings;
using SportAcademy.Application.Queries.AuthQueries.ValidateInvitation;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class ValidateInvitationQueryHandlerTests
{
    private readonly Mock<IInvitationRepository> _invitationRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly ValidateInvitationQueryHandler _handler;

    public ValidateInvitationQueryHandlerTests()
    {
        _handler = new ValidateInvitationQueryHandler(
            _invitationRepoMock.Object,
            _unitOfWorkMock.Object,
            _tokenServiceMock.Object);
    }

    private static Invitation CreateValidInvitation()
    {
        return new Invitation
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            Email = "owner@test.com",
            TokenHash = "hashed-token",
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Purpose = InvitationPurpose.OwnerSetup,
            InvitedByUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsSuccess()
    {
        var invitation = CreateValidInvitation();

        _tokenServiceMock
            .Setup(s => s.HashToken("valid-token"))
            .Returns("hashed-token");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _handler.Handle(
            new ValidateInvitationQuery("valid-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Email.Should().Be("owner@test.com");
        result.Data.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ReturnsFailureAndExpiresInvitation()
    {
        var invitation = CreateValidInvitation();
        invitation.ExpiresAt = DateTime.UtcNow.AddDays(-1);

        _tokenServiceMock
            .Setup(s => s.HashToken("expired-token"))
            .Returns("hashed-expired");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-expired", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _handler.Handle(
            new ValidateInvitationQuery("expired-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        invitation.Status.Should().Be(InvitationStatus.Expired);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ReturnsFailure()
    {
        _tokenServiceMock
            .Setup(s => s.HashToken("unknown-token"))
            .Returns("hashed-unknown");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invitation?)null);

        var result = await _handler.Handle(
            new ValidateInvitationQuery("unknown-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_AlreadyAcceptedToken_ReturnsFailure()
    {
        var invitation = CreateValidInvitation();
        invitation.Accept();

        _tokenServiceMock
            .Setup(s => s.HashToken("used-token"))
            .Returns("hashed-used");

        _invitationRepoMock
            .Setup(r => r.FindByTokenHashAsync("hashed-used", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitation);

        var result = await _handler.Handle(
            new ValidateInvitationQuery("used-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}
