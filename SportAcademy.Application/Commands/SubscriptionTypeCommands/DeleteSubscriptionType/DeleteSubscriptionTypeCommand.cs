using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.DeleteSubscriptionType
{
    public record DeleteSubscriptionTypeCommand(int Id) : IRequest<Result<bool>>;
}