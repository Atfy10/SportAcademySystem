using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.NotificationCommands.MarkAllNotificationsAsRead;
using SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Tests.Application.Handlers;

public class MarkNotificationAsReadCommandHandlerTests
{
    private static readonly Guid TestUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _userContextMock.Setup(s => s.UserId).Returns(TestUserId);
    }

    [Fact]
    public async Task Handle_ValidNotification_MarksAsRead()
    {
        var command = new MarkNotificationAsReadCommand(NotificationId: 1);
        var handler = new MarkNotificationAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAsReadAsync(1, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.MarkAsReadAsync(1, TestUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotificationNotFound_ReturnsFalse()
    {
        var command = new MarkNotificationAsReadCommand(NotificationId: 999);
        var handler = new MarkNotificationAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAsReadAsync(999, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentNotificationIds_MarksCorrectNotification(int notificationId)
    {
        var command = new MarkNotificationAsReadCommand(NotificationId: notificationId);
        var handler = new MarkNotificationAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAsReadAsync(notificationId, TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.MarkAsReadAsync(notificationId, TestUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class MarkAllNotificationsAsReadCommandHandlerTests
{
    private static readonly Guid TestUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();

    public MarkAllNotificationsAsReadCommandHandlerTests()
    {
        _userContextMock.Setup(s => s.UserId).Returns(TestUserId);
    }

    [Fact]
    public async Task Handle_ValidUser_MarksAllAsRead()
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(5);
        _notificationRepoMock.Verify(r => r.MarkAllAsReadAsync(TestUserId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoNotifications_ReturnsZero()
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    public async Task Handle_VariousNotificationCounts_ReturnsCorrectCount(int count)
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(count);
    }

    [Fact]
    public async Task Handle_UsesCorrectUserIdFromContext()
    {
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync(TestUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        await handler.Handle(command, CancellationToken.None);

        _notificationRepoMock.Verify(r => r.MarkAllAsReadAsync(TestUserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
