using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PushSubscriptionCommands.UnregisterPushSubscription;

// Deliberately no IRequiresFeature - a user must always be able to remove their own subscription
// (e.g. cleaning up after notifications-push was disabled), never blocked by the same gate that
// controls whether new ones can be created.
public record UnregisterPushSubscriptionCommand(string Endpoint) : IRequest<Result<bool>>;
