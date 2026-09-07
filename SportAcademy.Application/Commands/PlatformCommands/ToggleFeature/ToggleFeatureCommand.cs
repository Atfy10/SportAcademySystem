using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;

public record ToggleFeatureCommand(Guid TenantId, Guid FeatureId, bool IsEnabled) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.feature_toggled";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the existing TenantFeature row's state before toggling it -
    /// null when there is no existing row yet (the feature is being enabled for the first
    /// time).</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
