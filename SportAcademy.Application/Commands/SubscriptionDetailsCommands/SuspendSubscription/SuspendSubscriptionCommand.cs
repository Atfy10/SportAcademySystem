using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.SuspendSubscription
{
    public record SuspendSubscriptionCommand(int Id) : IRequest<Result<bool>>;
}
