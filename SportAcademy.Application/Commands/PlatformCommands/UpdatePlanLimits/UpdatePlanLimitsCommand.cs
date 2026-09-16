using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdatePlanLimits;

// The quantitative twin of UpdatePlanFeaturesCommand - replaces a plan's entire limit set at
// once and retroactively reconciles every tenant currently on it. A resource key missing from
// Limits means unlimited for that plan (see PlanLimit's own comment), not "leave unchanged" -
// the caller (the console's Limits tab) always sends the complete set.
public record UpdatePlanLimitsCommand(int PlanId, Dictionary<string, int?> Limits) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "platform.plan_limits_updated";

    // Same reasoning as UpdatePlanFeaturesCommand: a plan-wide edit has no single tenant target.
    Guid? IAuditableCommand.AuditTenantId => null;

    /// <summary>Set by the handler to the plan's previous limits before replacing them.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
