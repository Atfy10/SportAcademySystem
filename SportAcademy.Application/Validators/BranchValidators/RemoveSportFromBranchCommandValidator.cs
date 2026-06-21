using FluentValidation;
using SportAcademy.Application.Commands.BranchCommands.RemoveSportFromBranch;

namespace SportAcademy.Application.Validators.BranchValidators
{
    public class RemoveSportFromBranchCommandValidator : AbstractValidator<RemoveSportFromBranchCommand>
    {
        public RemoveSportFromBranchCommandValidator()
        {
            RuleFor(x => x.SportId)
                .GreaterThan(0).WithMessage("SportId must be greater than 0.");

            RuleFor(x => x.BranchId)
                .GreaterThan(0).WithMessage("BranchId must be greater than 0.");
        }
    }
}
