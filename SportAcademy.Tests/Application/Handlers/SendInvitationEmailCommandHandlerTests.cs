using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.AuthCommands.SendInvitationEmail;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class SendInvitationEmailCommandHandlerTests
{
    private readonly Mock<IInvitationRepository> _invitationRepoMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IBaseRepository<Tenant, Guid>> _tenantRepoMock = new();
    private readonly Mock<IAppUrlProvider> _appUrlProviderMock = new();
    private readonly Mock<IInvitationEmailSender> _emailSenderMock = new();
    private readonly SendInvitationEmailCommandHandler _handler;

    public SendInvitationEmailCommandHandlerTests()
    {
        _handler = new SendInvitationEmailCommandHandler(
            _invitationRepoMock.Object, _tokenServiceMock.Object, _tenantRepoMock.Object,
            _appUrlProviderMock.Object, _emailSenderMock.Object);
    }

    [Fact]
    public async Task Handle_ValidInvitation_SendsTheExactSameLinkTheTokenResolvesTo()
    {
        var tenantId = Guid.NewGuid();
        var invitation = new Invitation { TenantId = tenantId, Email = "owner@test.com", Status = InvitationStatus.Pending };
        var tenant = new Tenant { Id = tenantId, Name = "Test", Slug = "test-academy" };

        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _appUrlProviderMock.Setup(p => p.InvitationUrl("test-academy", "raw-token")).Returns("https://app.test/t/test-academy/invite/raw-token");

        var result = await _handler.Handle(new SendInvitationEmailCommand(tenantId, "raw-token"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _emailSenderMock.Verify(
            s => s.SendInvitationLinkAsync("owner@test.com", "https://app.test/t/test-academy/invite/raw-token", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_TokenBelongsToADifferentTenant_ReturnsNotFound()
    {
        var invitation = new Invitation { TenantId = Guid.NewGuid(), Email = "owner@test.com", Status = InvitationStatus.Pending };
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _handler.Handle(new SendInvitationEmailCommand(Guid.NewGuid(), "raw-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        _emailSenderMock.Verify(s => s.SendInvitationLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyAcceptedInvitation_ReturnsFailure()
    {
        var tenantId = Guid.NewGuid();
        var invitation = new Invitation { TenantId = tenantId, Email = "owner@test.com", Status = InvitationStatus.Accepted };
        _tokenServiceMock.Setup(s => s.HashToken("raw-token")).Returns("hashed-token");
        _invitationRepoMock.Setup(r => r.FindByTokenHashAsync("hashed-token", It.IsAny<CancellationToken>())).ReturnsAsync(invitation);

        var result = await _handler.Handle(new SendInvitationEmailCommand(tenantId, "raw-token"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _emailSenderMock.Verify(s => s.SendInvitationLinkAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
