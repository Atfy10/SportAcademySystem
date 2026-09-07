using FluentAssertions;
using Moq;
using SportAcademy.Application.EventHandlers;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Tests.Application.Handlers;

public class ImpersonationStartedHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly ImpersonationStartedHandler _handler;

    public ImpersonationStartedHandlerTests()
    {
        _handler = new ImpersonationStartedHandler(_userRepoMock.Object, _notificationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_NotifiesTheOwnerWithTheSuperAdminsNameAndReason()
    {
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var superAdminId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);
        var notification = new ImpersonationStartedEvent(tenantId, ownerId, superAdminId, "Support ticket #123", expiresAt);

        _userRepoMock
            .Setup(r => r.GetDisplayNameAsync(superAdminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("John Admin");

        await _handler.Handle(notification, CancellationToken.None);

        _notificationServiceMock.Verify(
            n => n.SendNotificationAsync(
                ownerId.ToString(),
                It.IsAny<string>(),
                It.Is<string>(m => m.Contains("John Admin") && m.Contains("Support ticket #123")),
                NotificationType.Warning,
                It.IsAny<string?>()),
            Times.Once);
    }
}
