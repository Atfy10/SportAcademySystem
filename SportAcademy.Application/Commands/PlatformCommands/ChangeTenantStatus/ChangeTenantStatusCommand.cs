using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus;

public record ChangeTenantStatusCommand(Guid TenantId, TenantStatus NewStatus, string? Reason = null)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.status_changed";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the tenant's Status before transitioning it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
