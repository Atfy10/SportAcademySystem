using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant;

public record ArchiveTenantCommand(Guid TenantId, string Reason) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.archived";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the tenant's Status before archiving it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
