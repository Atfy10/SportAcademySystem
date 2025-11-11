using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{
    public class UpdateSubscriptionDetailsValidator : AbstractValidator<UpdateSubscriptionDetailsCommand>
    {
        public UpdateSubscriptionDetailsValidator() 
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Id)
                .ApplyIdRuleFor("Subscription Details");

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
                .ApplyOptionalIdRuleFor("Trainee");

            RuleFor(x => x.SubscriptionTypeId)
                .ApplyOptionalIdRuleFor("Subscription Type");

            RuleFor(x => x.SportId)
                .ApplyOptionalIdRuleFor("Sport");

            RuleFor(x => x.BranchId)
                .ApplyOptionalIdRuleFor("Branch");
        }
    }
}
