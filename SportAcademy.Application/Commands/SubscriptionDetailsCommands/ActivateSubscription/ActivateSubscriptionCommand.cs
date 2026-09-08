using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.ActivateSubscription
{
    public record ActivateSubscriptionCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "enrollment-management";
    }
}
