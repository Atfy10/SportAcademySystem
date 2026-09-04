using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.CreateSubscriptionDiscountRequest;

namespace SportAcademy.Application.Validators.SubscriptionDiscountRequestValidators
{
    public class CreateSubscriptionDiscountRequestValidator : AbstractValidator<CreateSubscriptionDiscountRequestCommand>
    {
        public CreateSubscriptionDiscountRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Please select a start date.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Please select an end date.")
                .GreaterThan(x => x.StartDate).WithMessage("End date should be after the start date.");

            RuleFor(x => x.TraineeId).ApplyIdRuleFor("Trainee");
            RuleFor(x => x.SubscriptionTypeId).ApplyIdRuleFor("Subscription Type");
            RuleFor(x => x.SportId).ApplyIdRuleFor("Sport");
            RuleFor(x => x.BranchId).ApplyIdRuleFor("Branch");

            RuleFor(x => x.PaymentTypeId).GreaterThan(0).WithMessage("A payment type must be selected.");

            RuleFor(x => x.DiscountCode)
                .NotEmpty().WithMessage("A discount code is required.")
                .MaximumLength(30).WithMessage("Discount code must not exceed 30 characters.");
        }
    }
}
