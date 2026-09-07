using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ExtendTenantSubscription;

public record ExtendTenantSubscriptionCommand(Guid TenantId, int Days) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.subscription_extended";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the subscription's EndsAt before extending it.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
