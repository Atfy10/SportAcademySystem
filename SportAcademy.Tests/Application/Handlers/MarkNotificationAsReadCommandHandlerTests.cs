using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.NotificationCommands.MarkAllNotificationsAsRead;
using SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Tests.Application.Handlers;

public class MarkNotificationAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _userContextMock.Setup(s => s.UserId).Returns("test-user-123");
    }

    [Fact]
    public async Task Handle_ValidNotification_MarksAsRead()
    {
        // Arrange
        var command = new MarkNotificationAsReadCommand(NotificationId: 1);
        var handler = new MarkNotificationAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAsReadAsync(1, "test-user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.MarkAsReadAsync(1, "test-user-123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotificationNotFound_ReturnsFalse()
    {
        // Arrange
        var command = new MarkNotificationAsReadCommand(NotificationId: 999);
        var handler = new MarkNotificationAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAsReadAsync(999, "test-user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentNotificationIds_MarksCorrectNotification(int notificationId)
    {
        // Arrange
        var command = new MarkNotificationAsReadCommand(NotificationId: notificationId);
        var handler = new MarkNotificationAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAsReadAsync(notificationId, "test-user-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.MarkAsReadAsync(notificationId, "test-user-123", It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class MarkAllNotificationsAsReadCommandHandlerTests
{
    private readonly Mock<INotificationRepository> _notificationRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();

    public MarkAllNotificationsAsReadCommandHandlerTests()
    {
        _userContextMock.Setup(s => s.UserId).Returns("test-user-456");
    }

    [Fact]
    public async Task Handle_ValidUser_MarksAllAsRead()
    {
        // Arrange
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync("test-user-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(5);
        _notificationRepoMock.Verify(r => r.MarkAllAsReadAsync("test-user-456", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoNotifications_ReturnsZero()
    {
        // Arrange
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync("test-user-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    public async Task Handle_VariousNotificationCounts_ReturnsCorrectCount(int count)
    {
        // Arrange
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync("test-user-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(count);
    }

    [Fact]
    public async Task Handle_UsesCorrectUserIdFromContext()
    {
        // Arrange
        var command = new MarkAllNotificationsAsReadCommand();
        var handler = new MarkAllNotificationsAsReadCommandHandler(_notificationRepoMock.Object, _userContextMock.Object);

        _notificationRepoMock.Setup(r => r.MarkAllAsReadAsync("test-user-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _notificationRepoMock.Verify(r => r.MarkAllAsReadAsync("test-user-456", It.IsAny<CancellationToken>()), Times.Once);
    }
}
