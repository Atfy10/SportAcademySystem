using MediatR;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(int NotificationId) : IRequest<bool>;
}
