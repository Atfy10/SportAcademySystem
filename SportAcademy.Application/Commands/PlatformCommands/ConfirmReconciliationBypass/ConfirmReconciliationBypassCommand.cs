using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ConfirmReconciliationBypass;

// Step 2: the code from RequestReconciliationBypassCommand's email confirms this really is a
// deliberate SuperAdmin action, not a misclick. On success the tenant returns to Active WITHOUT
// its over-limit condition being resolved - Reason is required so the audit trail (this is an
// IAuditableCommand, unlike the request step) carries the "why" a future reader will need,
// exactly like SetTenantLimitOverrideCommand's Reason does for the same kind of exception.
public record ConfirmReconciliationBypassCommand(Guid TenantId, string Code, string Reason)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "platform.limit_reconciliation_bypassed";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the resources that were still over cap at the moment of
    /// bypass - the tenant is not reconciled, only unlocked, so this is what a future reader
    /// needs to know was left unresolved.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
