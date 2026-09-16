using FluentValidation;
using SportAcademy.Application.Common.Limits;

namespace SportAcademy.Application.Commands.PlatformCommands.SetTenantLimitOverride;

public class SetTenantLimitOverrideCommandValidator : AbstractValidator<SetTenantLimitOverrideCommand>
{
    public SetTenantLimitOverrideCommandValidator()
    {
        RuleFor(x => x.ResourceKey)
            .Must(LimitedResources.IsKnown)
            .WithMessage("Unknown resource key.");

        RuleFor(x => x.MaxCount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MaxCount.HasValue);

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MinimumLength(5)
            .MaximumLength(500);
    }
}
