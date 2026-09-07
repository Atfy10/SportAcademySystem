using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.SetTenantTrial;

public record SetTenantTrialCommand(Guid TenantId) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.trial_set";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the subscription's trial/renewal fields before setting the
    /// trial.</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
