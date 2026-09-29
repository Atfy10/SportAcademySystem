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
                .NotEmpty().WithMessage("Please select a start date.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Please select an end date.")
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date should be after the start date.");

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
