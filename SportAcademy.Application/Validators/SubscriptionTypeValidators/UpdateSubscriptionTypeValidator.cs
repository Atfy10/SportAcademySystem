using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.UpdateSubscriptionType;

namespace SportAcademy.Application.Validators.SubscriptionTypeValidators
{
    public class UpdateSubscriptionTypeValidator : AbstractValidator<UpdateSubscriptionTypeCommand>
    {
        public UpdateSubscriptionTypeValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            When(x => x.Name is not null, () =>
            {
                RuleFor(x => x.Name!)
                    .NotEmpty().WithMessage("Name must not be empty.")
                    .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");
            });

            When(x => x.DaysPerMonth.HasValue, () =>
            {
                RuleFor(x => x.DaysPerMonth!.Value)
                    .GreaterThan(0).WithMessage("Days per month must be greater than 0.")
                    .LessThanOrEqualTo(31).WithMessage("Days per month must not exceed 31.");
            });

            When(x => x.NumberOfMonths.HasValue, () =>
            {
                RuleFor(x => x.NumberOfMonths!.Value)
                    .GreaterThan(0).WithMessage("Number of months must be greater than 0.");
            });
        }
    }
}