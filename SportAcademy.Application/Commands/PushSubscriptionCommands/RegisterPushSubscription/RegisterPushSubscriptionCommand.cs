using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PushSubscriptionCommands.RegisterPushSubscription;

public record RegisterPushSubscriptionCommand(
    string Endpoint,
    string P256dh,
    string Auth,
    string? UserAgent
) : IRequest<Result<bool>>, IRequiresFeature
{
    public string FeatureKey => "notifications-push";
}
