using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;

public record ChangeTenantPlanCommand(Guid TenantId, int NewPlanId) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.plan_changed";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the subscription's current plan id before changing it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
