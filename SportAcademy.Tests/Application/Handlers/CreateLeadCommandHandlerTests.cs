using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.MarketingCommands.CreateLead;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Marketing;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class CreateLeadCommandHandlerTests
{
    private readonly Mock<IBaseRepository<Lead, Guid>> _leadRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IMarketingSettingsProvider> _marketingSettingsMock = new();
    private readonly CreateLeadCommandHandler _handler;

    public CreateLeadCommandHandlerTests()
    {
        _marketingSettingsMock.Setup(m => m.SalesInboxEmail).Returns("sales@auraacademys.com");
        _handler = new CreateLeadCommandHandler(
            _leadRepoMock.Object, _unitOfWorkMock.Object, _emailServiceMock.Object, _marketingSettingsMock.Object);
    }

    private static CreateLeadCommand ValidCommand(string? honeypot = null, DateTime? renderedAt = null) => new(
        FullName: "Ahmed Ali",
        AcademyName: "Champions Academy",
        Email: "ahmed@example.com",
        PhoneNumber: "201000000000",
        City: "Cairo",
        BranchCount: 2,
        TraineeCountBand: 1,
        Message: "Interested in a demo",
        Locale: "en",
        SourcePage: "/pricing",
        UtmSource: "google",
        UtmMedium: "cpc",
        UtmCampaign: "launch",
        Referrer: "https://google.com",
        IpHash: "abc123",
        HoneypotValue: honeypot,
        FormRenderedAtUtc: renderedAt);

    [Fact]
    public async Task Handle_RealSubmission_PersistsAsNewAndSendsBothEmails()
    {
        var command = ValidCommand(renderedAt: DateTime.UtcNow.AddSeconds(-10));

        Lead? captured = null;
        _leadRepoMock.Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((l, _) => captured = l)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        captured.Should().NotBeNull();
        captured!.Status.Should().Be(LeadStatus.New);

        _emailServiceMock.Verify(e => e.SendAsync(
            "sales@auraacademys.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendAsync(
            command.Email, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_HoneypotFilled_PersistsAsSpamAndSendsNoEmail()
    {
        var command = ValidCommand(honeypot: "http://spam.example");

        Lead? captured = null;
        _leadRepoMock.Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((l, _) => captured = l)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        // Same success response as a real submission - a bot must never learn its submission
        // was flagged.
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        captured!.Status.Should().Be(LeadStatus.Spam);

        _emailServiceMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SubmittedFasterThanAHumanCan_PersistsAsSpam()
    {
        var command = ValidCommand(renderedAt: DateTime.UtcNow.AddSeconds(-1));

        Lead? captured = null;
        _leadRepoMock.Setup(r => r.AddAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>()))
            .Callback<Lead, CancellationToken>((l, _) => captured = l)
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        captured!.Status.Should().Be(LeadStatus.Spam);
        _emailServiceMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmailServiceThrows_StillReturnsSuccess()
    {
        var command = ValidCommand(renderedAt: DateTime.UtcNow.AddSeconds(-10));
        _emailServiceMock.Setup(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Resend is down"));

        var result = await _handler.Handle(command, CancellationToken.None);

        // The lead row is already saved - an email provider outage must not turn an
        // otherwise-successful submission into a failure for the visitor.
        result.IsSuccess.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
