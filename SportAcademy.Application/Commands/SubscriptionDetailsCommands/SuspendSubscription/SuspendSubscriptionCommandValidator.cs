using FluentValidation;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.SuspendSubscription
{
    public class SuspendSubscriptionCommandValidator : AbstractValidator<SuspendSubscriptionCommand>
    {
        public SuspendSubscriptionCommandValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }
}
