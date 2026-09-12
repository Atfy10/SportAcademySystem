using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateFeatureBundlePricing
{
    public class UpdateFeatureBundlePricingCommandValidator : AbstractValidator<UpdateFeatureBundlePricingCommand>
    {
        public UpdateFeatureBundlePricingCommandValidator()
        {
            RuleFor(x => x.BundlePrice).GreaterThanOrEqualTo(0);
        }
    }
}
