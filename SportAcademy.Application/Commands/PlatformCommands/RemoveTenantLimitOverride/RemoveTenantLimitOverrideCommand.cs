using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.RemoveTenantLimitOverride;

// "Reset to plan" - removes the exception entirely so the tenant falls back to whatever its
// current plan grants for this resource, rather than setting the override to match the plan's
// number (which would leave a stale override row masking future plan changes).
public record RemoveTenantLimitOverrideCommand(Guid TenantId, string ResourceKey) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.limit_override_removed";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
