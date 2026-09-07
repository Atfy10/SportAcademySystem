using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateTenant;

public record UpdateTenantCommand(
    Guid TenantId,
    string? Name = null,
    string? DisplayName = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? Website = null,
    string? Description = null,
    string? TimeZone = null,
    string? Language = null,
    string? Currency = null
) : IRequest<Result<TenantDetailResponse>>, IAuditableCommand
{
    public string AuditEventType => "tenant.updated";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to every field this command can change, read from the tenant
    /// before any of them are applied.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
