using FluentValidation;
using SportAcademy.Application.Commands.CoachCommands.UpdateCoachBranches;

namespace SportAcademy.Application.Validators.CoachValidators;

public class UpdateCoachBranchesValidator : AbstractValidator<UpdateCoachBranchesCommand>
{
    public UpdateCoachBranchesValidator()
    {
        RuleFor(x => x.CoachId)
            .ApplyIdRuleFor("Coach");

        RuleFor(x => x.Branches)
            .NotEmpty().WithMessage("A coach must be authorized for at least one branch.");

        RuleForEach(x => x.Branches).ChildRules(branch =>
        {
            branch.RuleFor(b => b.BranchId).ApplyIdRuleFor("Branch");
            branch.RuleFor(b => b.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero.")
                .When(b => b.Salary.HasValue);
        });
    }
}
