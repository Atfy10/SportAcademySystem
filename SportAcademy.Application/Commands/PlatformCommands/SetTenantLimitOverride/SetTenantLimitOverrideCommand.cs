using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.SetTenantLimitOverride;

// The quantitative twin of ToggleFeatureCommand's Lock: true - a SuperAdmin-set exception for
// one tenant on one resource, always winning over the plan (in either direction - see
// TenantLimitOverride's own comment). Reason is required (unlike ToggleFeatureCommand's Lock,
// which needs none) because a numeric exception is inherently a negotiated, one-off decision
// that the next SuperAdmin looking at this tenant needs context for - a locked feature toggle is
// self-explanatory, a "this academy gets 3 branches on the Basic plan" number is not.
public record SetTenantLimitOverrideCommand(Guid TenantId, string ResourceKey, int? MaxCount, string Reason)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.limit_override_set";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the existing override's state before replacing it - null
    /// when there is no existing override yet.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
