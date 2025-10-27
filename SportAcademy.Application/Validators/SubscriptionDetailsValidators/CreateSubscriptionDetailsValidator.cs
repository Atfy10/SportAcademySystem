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
                .NotEmpty().WithMessage("Please choose a trainee.")
                .GreaterThan(0)
                .WithMessage("Selected trainee is not valid.");

            RuleFor(x => x.SubscriptionTypeId)
                .NotEmpty().WithMessage("Please choose a subscription type.")
                .GreaterThan(0)
                .WithMessage("Selected subscription type is not valid.");

            RuleFor(x => x.SportId)
                .NotEmpty().WithMessage("Please choose a sport.")
                .GreaterThan(0)
                .WithMessage("Selected sport is not valid.");

            RuleFor(x => x.BranchId)
                .NotEmpty().WithMessage("Please choose a branch.")
                .GreaterThan(0)
                .WithMessage("Selected branch is not valid.");
        }
    }
}
