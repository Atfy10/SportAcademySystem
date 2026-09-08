using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkAllNotificationsAsRead
{
    public record MarkAllNotificationsAsReadCommand : IRequest<Result<int>>, IRequiresFeature
    {
        public string FeatureKey => "notifications";
    }
}
