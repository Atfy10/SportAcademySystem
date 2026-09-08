using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdatePlanFeatures;

public record UpdatePlanFeaturesCommand(int PlanId, List<Guid> FeatureIds) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "platform.plan_features_updated";

    // No single tenant is the target of a plan-wide edit - TenantAuditEvent.TenantId is
    // explicitly nullable for exactly this case ("a platform-wide event with no single target
    // tenant").
    Guid? IAuditableCommand.AuditTenantId => null;

    /// <summary>Set by the handler to the plan's previous feature id list before replacing it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
