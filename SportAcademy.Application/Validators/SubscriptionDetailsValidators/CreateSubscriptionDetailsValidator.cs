using FluentValidation;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Validators.SubscriptionDetailsValidators
{

    public class CreateSubscriptionDetailsValidator : AbstractValidator<CreateSubscriptionDetailsCommand>
    {
        public CreateSubscriptionDetailsValidator(ISportPriceRepository sportPriceRepository)
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

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var exists = await sportPriceRepository.IsExistAsync(cmd.BranchId, cmd.SportId, cmd.SubscriptionTypeId, ct);
                    return exists;
                })
                .WithMessage("No price configured for this sport, branch, and plan combination.");
        }
    }
}
