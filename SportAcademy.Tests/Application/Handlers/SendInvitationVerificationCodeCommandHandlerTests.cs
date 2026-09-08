using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.SendInvitationVerificationCode;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class SendInvitationVerificationCodeCommandHandlerTests
{
    private readonly Mock<IInvitationRepository> _invitationRepoMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IInvitationEmailSender> _emailSenderMock = new();
    private readonly SendInvitationVerificationCodeCommandHandler _handler;

    public SendInvitationVerificationCodeCommandHandlerTests()
    {
        _handler = new SendInvitationVerificationCodeCommandHandler(
            _invitationRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object, _emailSenderMock.Object);
    }

    [Fact]
    public async Task Handle_PendingInvitation_GeneratesAndEmailsACode()
    {
        var invitation = new Invitation
        {
            Email = "invitee@test.com", Status = InvitationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddDays(1),
        };
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tokenServiceMock.Setup(s => s.GenerateNumericCode(6)).Returns("482910");
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("hashed-code");

        var result = await _handler.Handle(new SendInvitationVerificationCodeCommand("raw-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invitation.VerificationCodeHash.Should().Be("hashed-code");
        invitation.VerificationCodeExpiresAt.Should().NotBeNull();
        invitation.VerificationCodeAttempts.Should().Be(0);
        _emailSenderMock.Verify(s => s.SendVerificationCodeAsync("invitee@test.com", "482910", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AlreadyAcceptedInvitation_ReturnsFailureWithoutSendingAnything()
    {
        var invitation = new Invitation { Email = "invitee@test.com", Status = InvitationStatus.Accepted, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _handler.Handle(new SendInvitationVerificationCodeCommand("raw-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _emailSenderMock.Verify(s => s.SendVerificationCodeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ResendingReplacesTheOldCodeAndResetsAttempts()
    {
        var invitation = new Invitation
        {
            Email = "invitee@test.com", Status = InvitationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddDays(1),
            VerificationCodeHash = "old-hash", VerificationCodeAttempts = 4,
        };
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tokenServiceMock.Setup(s => s.GenerateNumericCode(6)).Returns("111111");
        _tokenServiceMock.Setup(s => s.HashToken("111111")).Returns("new-hash");

        await _handler.Handle(new SendInvitationVerificationCodeCommand("raw-token"), CancellationToken.None);

        invitation.VerificationCodeHash.Should().Be("new-hash");
        invitation.VerificationCodeAttempts.Should().Be(0);
    }
}
