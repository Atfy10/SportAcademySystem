using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateSubscriptionPlan
{
    public class UpdateSubscriptionPlanCommandValidator : AbstractValidator<UpdateSubscriptionPlanCommand>
    {
        public UpdateSubscriptionPlanCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
            RuleFor(x => x.Description).MaximumLength(1000);
            RuleFor(x => x.MonthlyPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.YearlyPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
        }
    }
}
