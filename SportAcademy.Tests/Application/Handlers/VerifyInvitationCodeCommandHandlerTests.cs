using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.VerifyInvitationCode;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class VerifyInvitationCodeCommandHandlerTests
{
    private readonly Mock<IInvitationRepository> _invitationRepoMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VerifyInvitationCodeCommandHandler _handler;

    public VerifyInvitationCodeCommandHandlerTests()
    {
        _handler = new VerifyInvitationCodeCommandHandler(_invitationRepoMock.Object, _tokenServiceMock.Object, _unitOfWorkMock.Object);
    }

    private static Invitation PendingInvitationWithCode(string codeHash, int attempts = 0) => new()
    {
        Status = InvitationStatus.Pending,
        ExpiresAt = DateTime.UtcNow.AddDays(1),
        VerificationCodeHash = codeHash,
        VerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(10),
        VerificationCodeAttempts = attempts,
    };

    [Fact]
    public async Task Handle_CorrectCode_MarksVerifiedAndClearsTheCode()
    {
        var invitation = PendingInvitationWithCode("correct-hash");
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("correct-hash");

        var result = await _handler.Handle(new VerifyInvitationCodeCommand("raw-token", "482910"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        invitation.IsEmailVerified.Should().BeTrue();
        invitation.VerificationCodeHash.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WrongCode_IncrementsAttemptsAndDoesNotVerify()
    {
        var invitation = PendingInvitationWithCode("correct-hash");
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tokenServiceMock.Setup(s => s.HashToken("000000")).Returns("wrong-hash");

        var result = await _handler.Handle(new VerifyInvitationCodeCommand("raw-token", "000000"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        invitation.IsEmailVerified.Should().BeFalse();
        invitation.VerificationCodeAttempts.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AlreadyAtMaxAttempts_RejectsEvenTheCorrectCode()
    {
        // Once locked out, only requesting a fresh code (which resets the counter) can recover -
        // not just eventually guessing right.
        var invitation = PendingInvitationWithCode("correct-hash", attempts: 5);
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("correct-hash");

        var result = await _handler.Handle(new VerifyInvitationCodeCommand("raw-token", "482910"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        invitation.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ExpiredCode_ReturnsFailure()
    {
        var invitation = PendingInvitationWithCode("correct-hash");
        invitation.VerificationCodeExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("correct-hash");

        var result = await _handler.Handle(new VerifyInvitationCodeCommand("raw-token", "482910"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        invitation.IsEmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoCodeEverRequested_ReturnsFailure()
    {
        var invitation = new Invitation { Status = InvitationStatus.Pending, ExpiresAt = DateTime.UtcNow.AddDays(1) };
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _handler.Handle(new VerifyInvitationCodeCommand("raw-token", "482910"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
