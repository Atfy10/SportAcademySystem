using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType
{
    public record CreateSubscriptionTypeCommand(
        string Name,
        int DaysPerMonth,
        int NumberOfMonths,
        bool IsActive,
        bool IsOffer,
        List<int> SportIds
    ) : IRequest<Result<int>>;
}