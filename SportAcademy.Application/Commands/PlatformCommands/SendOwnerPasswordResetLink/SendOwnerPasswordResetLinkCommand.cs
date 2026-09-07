using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.SendOwnerPasswordResetLink;

public record SendOwnerPasswordResetLinkCommand(Guid OwnerUserId) : IRequest<Result<bool>>, IAuditableCommand
{
    public string AuditEventType => "owner.password_reset_sent";

    /// <summary>Set by the handler once the owner is resolved - see BanOwnerCommand for why
    /// this needs its own settable property instead of a TenantId on the command.</summary>
    public Guid? ResolvedTenantId { get; set; }

    Guid? IAuditableCommand.AuditTenantId => ResolvedTenantId;

    // No "before" - this command emails a reset link but doesn't mutate the owner record itself
    // (the token is derived from the user's security stamp, not stored), so there is nothing an
    // entity snapshot would capture.
    object? IAuditableCommand.AuditBeforeState => null;
}
