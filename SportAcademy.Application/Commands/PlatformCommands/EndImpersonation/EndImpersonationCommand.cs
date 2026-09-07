using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.EndImpersonation;

public record EndImpersonationCommand(Guid GrantId) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "impersonation.ended";

    /// <summary>Set by the handler once the grant is resolved - the command only carries the
    /// grant id, not the tenant it targets.</summary>
    public Guid? ResolvedTenantId { get; set; }

    Guid? IAuditableCommand.AuditTenantId => ResolvedTenantId;

    /// <summary>Set by the handler to the grant's EndedAt/EndedReason before ending it (both
    /// null on a grant that's still active).</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
