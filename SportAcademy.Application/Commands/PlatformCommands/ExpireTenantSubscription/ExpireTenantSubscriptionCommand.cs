using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ExpireTenantSubscription;

public record ExpireTenantSubscriptionCommand(Guid TenantId) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.subscription_expired";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the subscription's EndsAt before expiring it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
