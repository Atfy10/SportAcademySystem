using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PublicDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateFeatureBundlePricing;

// Prices one feature block for the public bundle-builder - distinct from UpdatePlanFeatures
// (which grants) and UpdateSubscriptionPlan (which prices a whole plan). IsCore=true makes the
// feature pre-selected and locked in the builder UI (its price folds into the base total).
public record UpdateFeatureBundlePricingCommand(
    Guid FeatureId,
    decimal BundlePrice,
    bool IsCore
) : IRequest<Result<BundleFeatureDto>>, IAuditableCommand
{
    public string AuditEventType => "platform.feature_bundle_pricing_updated";
    Guid? IAuditableCommand.AuditTenantId => null;

    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
