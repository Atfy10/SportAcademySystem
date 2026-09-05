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
                .Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)))
                .WithMessage("Start date can’t be more than 30 days from today.");

            // No EndDate rule - it's derived from TrainingDays and the plan's session count in
            // SubscriptionCreationService, never submitted by the client.
            RuleFor(x => x.GroupType)
                .IsInEnum().WithMessage("Please choose whether this is public or private training.");

            RuleFor(x => x.TrainingDays)
                .NotEmpty().WithMessage("Please choose the training days - the subscription's end date is counted across them.")
                .Must(days => days.Distinct().Count() == days.Count)
                .WithMessage("The same training day was selected more than once.")
                .Must(days => days.Count <= 7)
                .WithMessage("A week only has seven days.");

            RuleFor(x => x.TraineeId)
                .ApplyIdRuleFor("Trainee");

            RuleFor(x => x.SubscriptionTypeId)
                .ApplyIdRuleFor("Subscription Type");

            RuleFor(x => x.SportId)
                .ApplyIdRuleFor("Sport");

            RuleFor(x => x.BranchId)
                .ApplyIdRuleFor("Branch");

            RuleFor(x => x.PaymentTypeId)
                .GreaterThan(0).WithMessage("A payment type must be selected.");

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var exists = await sportPriceRepository.IsExistAsync(cmd.BranchId, cmd.SportId, cmd.SubscriptionTypeId, cmd.GroupType, ct);
                    return exists;
                })
                .WithMessage("No price configured for this sport, branch, plan, and group type combination.");
        }
    }
}
