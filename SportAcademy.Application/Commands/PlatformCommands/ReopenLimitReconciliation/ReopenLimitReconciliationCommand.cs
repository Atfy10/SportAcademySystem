using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ReopenLimitReconciliation;

// SuperAdmin gives a tenant that lapsed into Suspended (LimitReconciliationDeadlineService) a
// fresh 7-day window. Deliberately its own command rather than a generic ChangeTenantStatusCommand
// call: reopening needs a NEW reconciliation snapshot/deadline, which ChangeTenantStatusCommand
// has no concept of - see that command's validator, which explicitly refuses to set
// PendingLimitSelection directly for exactly this reason.
public record ReopenLimitReconciliationCommand(Guid TenantId) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "platform.limit_reconciliation_reopened";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
