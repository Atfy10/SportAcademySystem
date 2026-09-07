using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.BanOwner;

public record BanOwnerCommand(Guid OwnerUserId, bool Banned) : IRequest<Result<bool>>, IAuditableCommand
{
    public string AuditEventType => Banned ? "owner.banned" : "owner.unbanned";

    /// <summary>Set by the handler once the owner is resolved - this command targets an owner,
    /// not a tenant, so there is no TenantId to read off it directly. Null if the owner wasn't
    /// found.</summary>
    public Guid? ResolvedTenantId { get; set; }

    Guid? IAuditableCommand.AuditTenantId => ResolvedTenantId;

    /// <summary>Set by the handler to the owner's IsBanned flag before flipping it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
