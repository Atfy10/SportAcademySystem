using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.SubmitLimitSelection;

// The Owner/Admin's answer to "you're over your new plan's limits - choose what survives" (see
// TenantStatus.PendingLimitSelection). Deliberately NOT scoped by a TenantId parameter - the
// caller can only ever complete their own tenant's reconciliation (TenantId comes from the JWT,
// see ResolvedTenantId below), exactly like TenantController's other self-service commands.
// Each list is the FULL authoritative set of what should survive of that type; everything else
// of that type is deactivated/banned. Trainees have no list here - they're grandfathered (D6),
// never part of this selection.
public record SubmitLimitSelectionCommand(List<int> BranchIds, List<int> SportIds, List<Guid> UserIds)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.limit_selection_submitted";

    /// <summary>Set by the handler from IUserContextService.TenantId - the command carries no
    /// tenant field of its own (a tenant can only ever act on itself), so PlatformAuditBehavior
    /// reads this back the same way it does for CreateTenantCommand/BanOwnerCommand.</summary>
    public Guid? ResolvedTenantId { get; set; }
    Guid? IAuditableCommand.AuditTenantId => ResolvedTenantId;

    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
