using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdatePlanFeatures
{
    public class UpdatePlanFeaturesCommandValidator : AbstractValidator<UpdatePlanFeaturesCommand>
    {
        public UpdatePlanFeaturesCommandValidator()
        {
            // An empty list is a legitimate choice (stripping a plan down to zero features), so
            // only null is rejected here - not a minimum count.
            RuleFor(x => x.FeatureIds).NotNull();
        }
    }
}
