using FluentValidation;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.ActivateSubscription
{
    public class ActivateSubscriptionCommandValidator : AbstractValidator<ActivateSubscriptionCommand>
    {
        public ActivateSubscriptionCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
