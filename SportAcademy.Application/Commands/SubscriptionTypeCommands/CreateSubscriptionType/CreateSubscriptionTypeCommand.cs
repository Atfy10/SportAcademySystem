using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType
{
    public record CreateSubscriptionTypeCommand(
        string Name,
        int DaysPerMonth,
        int NumberOfMonths,
        bool IsActive,
        bool IsOffer,
        List<int> SportIds
    ) : IRequest<Result<int>>, IRequiresFeature
    {
        public string FeatureKey => "subscription-plan";
    }
}