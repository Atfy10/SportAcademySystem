using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;

// Lock: the SuperAdmin's explicit choice on every toggle - true forces this value and prevents
// the tenant from changing it themselves (LockedBySuperAdmin), false just sets the value and
// leaves (or restores) the tenant's own ability to change it later. Toggling isn't inherently a
// lock-in decision; a SuperAdmin nudging a feature on/off for a tenant without permanently
// overriding their plan/self-service choice is a distinct, equally valid action.
public record ToggleFeatureCommand(Guid TenantId, Guid FeatureId, bool IsEnabled, bool Lock) : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.feature_toggled";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the existing TenantFeature row's state before toggling it -
    /// null when there is no existing row yet (the feature is being enabled for the first
    /// time).</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
