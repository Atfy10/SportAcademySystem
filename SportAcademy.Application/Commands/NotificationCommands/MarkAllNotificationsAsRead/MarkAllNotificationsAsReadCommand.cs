using MediatR;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkAllNotificationsAsRead
{
    public record MarkAllNotificationsAsReadCommand : IRequest<int>;
}
