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

            // No upper bound: a subscription can be sold today for any future start (it shows as
            // Upcoming until then). Past dates stay allowed for back-dating paper sign-ups.
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Please select a start date.");

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

            this.ApplyDepositRules(x => x.PayDeposit, x => x.DepositAmount, x => x.BalanceDueDate, x => x.DepositNote);

            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var exists = await sportPriceRepository.IsExistAsync(cmd.BranchId, cmd.SportId, cmd.SubscriptionTypeId, cmd.GroupType, ct);
                    return exists;
                })
                // Keyed to the plan field (an object-level rule otherwise reports under "", which no
                // form input can show) - the console offers "Set price now" right there.
                .OverridePropertyName(nameof(CreateSubscriptionDetailsCommand.SubscriptionTypeId))
                .WithMessage("No price configured for this sport, branch, plan, and group type combination.");

            // Deposit vs the real total. Only reachable once the price is known to exist (cascade
            // stops at the rule above otherwise).
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    var price = await sportPriceRepository.GetByKeyWithIncludesAsync(
                        cmd.BranchId, cmd.SportId, cmd.SubscriptionTypeId, cmd.GroupType, ct);
                    return price is null || cmd.DepositAmount < price.Price;
                })
                .When(x => x.PayDeposit && x.DepositAmount.HasValue)
                .WithName(nameof(CreateSubscriptionDetailsCommand.DepositAmount))
                .WithMessage("The deposit must be less than the subscription total. To pay everything now, untick \"Pay a deposit\".");
        }
    }
}
