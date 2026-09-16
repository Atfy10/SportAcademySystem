using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.ConfirmReconciliationBypass;

public class ConfirmReconciliationBypassCommandValidator : AbstractValidator<ConfirmReconciliationBypassCommand>
{
    public ConfirmReconciliationBypassCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(5).MaximumLength(500);
    }
}
