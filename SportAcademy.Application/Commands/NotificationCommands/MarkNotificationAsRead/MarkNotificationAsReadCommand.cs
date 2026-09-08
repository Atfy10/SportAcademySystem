using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead
{
    public record MarkNotificationAsReadCommand(int NotificationId) : IRequest<Result>, IRequiresFeature
    {
        public string FeatureKey => "notifications";
    }
}
