using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.CreateTenant;

public record CreateTenantCommand(
    string Name,
    string DisplayName,
    string Slug,
    string Code,
    string Email,
    string OwnerName,
    string OwnerEmail,
    int SubscriptionPlanId,
    string? Phone = null,
    string? Address = null,
    string? TimeZone = null,
    string? Language = null,
    string? Currency = null
) : IRequest<Result<TenantDetailResponse>>, IAuditableCommand
{
    public string AuditEventType => "tenant.created";

    /// <summary>Set by the handler once the new tenant's id is known - there is no TenantId to
    /// read off this command up front, since the tenant doesn't exist until the handler creates
    /// it. Null if the command failed before that point (a duplicate slug/code, say).</summary>
    public Guid? ResolvedTenantId { get; set; }

    Guid? IAuditableCommand.AuditTenantId => ResolvedTenantId;

    // No "before" - the tenant doesn't exist until this command creates it.
    object? IAuditableCommand.AuditBeforeState => null;
}
