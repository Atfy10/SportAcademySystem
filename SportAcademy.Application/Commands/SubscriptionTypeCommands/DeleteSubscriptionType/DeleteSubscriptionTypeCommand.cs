using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.DeleteSubscriptionType
{
    public record DeleteSubscriptionTypeCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "subscription-plan";
    }
}