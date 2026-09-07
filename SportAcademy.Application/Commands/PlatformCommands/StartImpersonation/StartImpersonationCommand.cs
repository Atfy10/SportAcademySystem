using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.StartImpersonation;

public record StartImpersonationCommand(Guid TenantId, string Reason)
    : IRequest<Result<ImpersonationSessionDto>>, IAuditableCommand
{
    public string AuditEventType => "impersonation.started";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    // No "before" - a grant is a brand-new record, there is no prior state to snapshot.
    object? IAuditableCommand.AuditBeforeState => null;
}
