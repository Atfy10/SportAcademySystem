using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.SubmitLimitSelection;

public class SubmitLimitSelectionCommandValidator : AbstractValidator<SubmitLimitSelectionCommand>
{
    public SubmitLimitSelectionCommandValidator()
    {
        // At least one branch must survive - an empty selection would lock the tenant (and
        // every one of its users) out of the console entirely, with no branch left to assign
        // anyone to.
        RuleFor(x => x.BranchIds).NotNull().Must(ids => ids.Count > 0)
            .WithMessage("At least one branch must remain active.");

        RuleFor(x => x.SportIds).NotNull();

        // At least one user (checked here structurally; the handler separately verifies at
        // least one of them is an Owner - see SubmitLimitSelectionCommandHandler).
        RuleFor(x => x.UserIds).NotNull().Must(ids => ids.Count > 0)
            .WithMessage("At least one user must remain active.");
    }
}
