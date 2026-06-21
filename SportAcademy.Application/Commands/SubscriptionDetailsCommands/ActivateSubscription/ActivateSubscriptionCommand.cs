using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.ActivateSubscription
{
    public record ActivateSubscriptionCommand(int Id) : IRequest<Result<bool>>;
}
