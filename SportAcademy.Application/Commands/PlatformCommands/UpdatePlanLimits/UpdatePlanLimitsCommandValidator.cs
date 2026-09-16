using FluentValidation;
using SportAcademy.Application.Common.Limits;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdatePlanLimits;

public class UpdatePlanLimitsCommandValidator : AbstractValidator<UpdatePlanLimitsCommand>
{
    public UpdatePlanLimitsCommandValidator()
    {
        RuleFor(x => x.Limits).NotNull();

        RuleFor(x => x.Limits)
            .Must(limits => limits.Keys.All(LimitedResources.IsKnown))
            .WithMessage("Unknown resource key.")
            .When(x => x.Limits is not null);

        RuleFor(x => x.Limits)
            .Must(limits => limits.Values.All(v => v is null || v >= 0))
            .WithMessage("Limit values must be zero or greater, or omitted for unlimited.")
            .When(x => x.Limits is not null);
    }
}
