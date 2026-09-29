using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.CreateSubscriptionDiscountRequest;
using SportAcademy.Application.Validators.SubscriptionDetailsValidators;

namespace SportAcademy.Application.Validators.SubscriptionDiscountRequestValidators
{
    public class CreateSubscriptionDiscountRequestValidator : AbstractValidator<CreateSubscriptionDiscountRequestCommand>
    {
        public CreateSubscriptionDiscountRequestValidator()
        {
            ClassLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Please select a start date.");

            // End date isn't submitted - it's computed at approval time from the plan's session
            // count walked across these training days (see SubscriptionCreationService).
            RuleFor(x => x.GroupType)
                .IsInEnum().WithMessage("Please choose whether this is public or private training.");

            RuleFor(x => x.TrainingDays)
                .NotEmpty().WithMessage("Please choose the training days - the subscription's end date is counted across them.")
                .Must(days => days.Distinct().Count() == days.Count)
                .WithMessage("The same training day was selected more than once.")
                .Must(days => days.Count <= 7)
                .WithMessage("A week only has seven days.");

            RuleFor(x => x.TraineeId).ApplyIdRuleFor("Trainee");
            RuleFor(x => x.SubscriptionTypeId).ApplyIdRuleFor("Subscription Type");
            RuleFor(x => x.SportId).ApplyIdRuleFor("Sport");
            RuleFor(x => x.BranchId).ApplyIdRuleFor("Branch");

            RuleFor(x => x.PaymentTypeId).GreaterThan(0).WithMessage("A payment type must be selected.");

            this.ApplyDepositRules(x => x.PayDeposit, x => x.DepositAmount, x => x.BalanceDueDate, x => x.DepositNote);

            RuleFor(x => x.DiscountCode)
                .NotEmpty().WithMessage("A discount code is required.")
                .MaximumLength(30).WithMessage("Discount code must not exceed 30 characters.");
        }
    }
}
