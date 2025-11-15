using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{

    public class CreateSubscriptionDetailsValidator : AbstractValidator<CreateSubscriptionDetailsCommand>
    {
        public CreateSubscriptionDetailsValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Please select a start date.")
                .Must(x => x <= DateOnly.FromDateTime(DateTime.Now.AddDays(30)))
                .WithMessage("Start date can’t be more than 30 days from today.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Please select an end date.")
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date should be after the start date.");

            RuleFor(x => x.PaymentNumber)
                .NotEmpty().WithMessage("Please enter a payment number.")
                .MaximumLength(50)
                .WithMessage("Payment number can’t be longer than 50 characters.");

            RuleFor(x => x.TraineeId)
                .ApplyIdRuleFor("Trainee");

            RuleFor(x => x.SubscriptionTypeId)
                .ApplyIdRuleFor("Subscription Type");

            RuleFor(x => x.SportId)
                .ApplyIdRuleFor("Sport");

            RuleFor(x => x.BranchId)
                .ApplyIdRuleFor("Branch");
        }
    }
}
