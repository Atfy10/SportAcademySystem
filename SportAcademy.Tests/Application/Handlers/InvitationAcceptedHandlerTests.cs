using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.EventHandlers;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Tests.Application.Handlers;

public class InvitationAcceptedHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly InvitationAcceptedHandler _handler;

    public InvitationAcceptedHandlerTests()
    {
        _handler = new InvitationAcceptedHandler(
            Mock.Of<ILogger<InvitationAcceptedHandler>>(),
            _unitOfWorkMock.Object,
            _userRepoMock.Object,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_SystemProvisionedInvitation_SkipsNotificationWithoutThrowing()
    {
        // TenantCreatedHandler stamps Guid.Empty as InvitedByUserId for the auto-provisioned
        // Owner invitation it creates when a SuperAdmin provisions a brand-new tenant - there is
        // no real inviter to notify. Notifying Guid.Empty would try to create a
        // NotificationRecipient for a user that doesn't exist, which EF cannot resolve.
        var notification = new InvitationAcceptedEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty);

        var act = () => _handler.Handle(notification, CancellationToken.None);

        await act.Should().NotThrowAsync();
        _notificationServiceMock.Verify(
            n => n.SendNotificationAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string?>()),
            Times.Never);
        _userRepoMock.Verify(r => r.GetDisplayNameAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_StaffInvitation_NotifiesTheRealInviter()
    {
        var invitedByUserId = Guid.NewGuid();
        var accepterId = Guid.NewGuid();
        var notification = new InvitationAcceptedEvent(Guid.NewGuid(), accepterId, invitedByUserId);

        _userRepoMock
            .Setup(r => r.GetDisplayNameAsync(accepterId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Jane Doe");

        await _handler.Handle(notification, CancellationToken.None);

        _notificationServiceMock.Verify(
            n => n.SendNotificationAsync(
                invitedByUserId.ToString(),
                "Invitation Accepted",
                It.Is<string>(m => m.Contains("Jane Doe")),
                NotificationType.Success,
                It.IsAny<string?>()),
            Times.Once);
    }
}
