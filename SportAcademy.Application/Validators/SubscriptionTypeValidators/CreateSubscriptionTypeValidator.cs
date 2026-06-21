using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType;

namespace SportAcademy.Application.Validators.SubscriptionTypeValidators
{
    public class CreateSubscriptionTypeValidator : AbstractValidator<CreateSubscriptionTypeCommand>
    {
        public CreateSubscriptionTypeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

            RuleFor(x => x.DaysPerMonth)
                .GreaterThan(0).WithMessage("Days per month must be greater than 0.")
                .LessThanOrEqualTo(31).WithMessage("Days per month must not exceed 31.");

            RuleFor(x => x.NumberOfMonths)
                .GreaterThan(0).WithMessage("Number of months must be greater than 0.");
        }
    }
}