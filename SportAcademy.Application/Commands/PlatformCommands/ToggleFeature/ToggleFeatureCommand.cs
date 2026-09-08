using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;

// Lock: the SuperAdmin's explicit choice on every toggle - true forces this value and prevents
// the tenant from changing it themselves (LockedBySuperAdmin), false just sets the value and
// leaves (or restores) the tenant's own ability to change it later. Toggling isn't inherently a
// lock-in decision; a SuperAdmin nudging a feature on/off for a tenant without permanently
// overriding their plan/self-service choice is a distinct, equally valid action.
//
// ConfirmProtectedDisable: required to be true to disable a FeatureDependencies.IsProtected
// feature (e.g. user-management) - the handler rejects the first attempt with a
// PROTECTED_FEATURE_CONFIRM_REQUIRED warning naming the risk, and the frontend re-sends with
// this set once the SuperAdmin explicitly confirms. Ignored for every non-protected feature and
// for enabling one.
public record ToggleFeatureCommand(Guid TenantId, Guid FeatureId, bool IsEnabled, bool Lock, bool ConfirmProtectedDisable = false)
    : IRequest<Result>, IAuditableCommand
{
    public string AuditEventType => "tenant.feature_toggled";
    Guid? IAuditableCommand.AuditTenantId => TenantId;

    /// <summary>Set by the handler to the existing TenantFeature row's state before toggling it -
    /// null when there is no existing row yet (the feature is being enabled for the first
    /// time).</summary>
    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
