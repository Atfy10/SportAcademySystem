using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.ClonePlan;

public class ClonePlanCommandValidator : AbstractValidator<ClonePlanCommand>
{
    public ClonePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OwnerTenantId).NotEmpty();
    }
}
