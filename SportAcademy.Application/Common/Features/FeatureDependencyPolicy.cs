namespace SportAcademy.Application.Common.Features;

// Enforces FeatureDependencies' edges at every point a feature's enabled state can change, so
// TenantFeature.IsEnabled=true always implies every prerequisite is also enabled - runtime
// request gating (FeatureGateBehavior) only ever needs to check a single feature's own flag
// because this invariant holds. Two distinct policies, matching who's making the change:
//
//  - Self-service (tenant Owner, via UpdateTenantFeatureCommand/BulkUpdateTenantFeaturesCommand):
//    BLOCK. A tenant's toggle must never silently ripple onto features they didn't touch.
//  - SuperAdmin (ToggleFeatureCommand) and plan authoring (UpdatePlanFeaturesCommand): CASCADE.
//    These are authoritative, whole-state operations - blocking them would just push the same
//    manual "now go enable the other three things" work onto an admin instead. A cascade never
//    overrides a LockedBySuperAdmin row in either direction; it's a prior explicit decision.
public static class FeatureDependencyPolicy
{
    // Self-service enable: every prerequisite must already be enabled. Returns null if OK, or an
    // error message naming what's missing.
    public static string? ValidateEnable(string featureName, IReadOnlySet<string> currentlyEnabledNames)
    {
        var missing = FeatureDependencies.GetPrerequisites(featureName)
            .Where(p => !currentlyEnabledNames.Contains(p))
            .ToList();

        return missing.Count == 0
            ? null
            : $"'{featureName}' requires {string.Join(", ", missing)} to be enabled first.";
    }

    // A protected feature (FeatureDependencies.IsProtected) can never be disabled through
    // self-service or automatic reconciliation - only a SuperAdmin can, and only with explicit
    // confirmation (see ToggleFeatureCommandHandler, which does NOT call this - a SuperAdmin's
    // path is a warn-and-confirm gate, not an unconditional block). Shared by every
    // Owner-facing / automatic disable path below.
    public static string? ValidateProtectedDisable(string featureName) =>
        FeatureDependencies.IsProtected(featureName)
            ? $"'{featureName}' is a protected platform capability and can't be disabled here. A SuperAdmin can disable it from the Platform console."
            : null;

    // Self-service disable: nothing currently enabled may still depend on it. Returns null if OK,
    // or an error message naming what's still depending on it.
    public static string? ValidateDisable(string featureName, IReadOnlySet<string> currentlyEnabledNames)
    {
        var protectedError = ValidateProtectedDisable(featureName);
        if (protectedError is not null)
            return protectedError;

        var blockers = FeatureDependencies.GetDependents(featureName)
            .Where(currentlyEnabledNames.Contains)
            .ToList();

        return blockers.Count == 0
            ? null
            : $"Can't disable '{featureName}': {string.Join(", ", blockers)} still depend on it.";
    }

    // Self-service bulk: validate a full desired end-state (every feature name the tenant has,
    // mapped to its post-save enabled value) is internally dependency-closed. Used by
    // BulkUpdateTenantFeaturesCommandHandler, which replaces many features' state at once - the
    // single-feature checks above aren't enough there since the payload can enable a feature and
    // its prerequisite in the same request.
    public static IReadOnlyList<string> ValidateEndState(IReadOnlyDictionary<string, bool> endStateByName)
    {
        var errors = new List<string>();

        foreach (var (name, isEnabled) in endStateByName)
        {
            if (!isEnabled)
            {
                var protectedError = ValidateProtectedDisable(name);
                if (protectedError is not null)
                    errors.Add(protectedError);
                continue;
            }

            var missing = FeatureDependencies.GetPrerequisites(name)
                .Where(p => !(endStateByName.TryGetValue(p, out var prereqEnabled) && prereqEnabled))
                .ToList();

            if (missing.Count > 0)
                errors.Add($"'{name}' requires {string.Join(", ", missing)} to be enabled.");
        }

        return errors;
    }

    // SuperAdmin single toggle: resolve what a cascade would need to also change. Never crosses a
    // LockedBySuperAdmin row - if the cascade would need to touch one, the whole operation is
    // reported as blocked instead, naming the conflicting feature(s).
    public static FeatureCascadeResult ResolveCascade(
        string featureName, bool targetEnabled, IReadOnlyDictionary<string, FeatureRuntimeState> currentStateByName)
    {
        var transitive = targetEnabled
            ? FeatureDependencies.GetTransitivePrerequisites(featureName)
            : FeatureDependencies.GetTransitiveDependents(featureName);

        var toChange = transitive
            .Where(name => currentStateByName.TryGetValue(name, out var state) && state.IsEnabled != targetEnabled)
            .ToList();

        var lockedConflicts = toChange
            .Where(name => currentStateByName[name].LockedBySuperAdmin)
            .ToList();

        return lockedConflicts.Count > 0
            ? FeatureCascadeResult.Conflict(lockedConflicts)
            : FeatureCascadeResult.Ok(toChange);
    }

    // Plan authoring: a plan's own feature set must be dependency-closed on its own, independent
    // of any tenant's current state - a plan that grants X without Y would hand every tenant on
    // it a permanently-unsatisfiable prerequisite. Returns validation errors, empty if closed.
    public static IReadOnlyList<string> ValidatePlanFeatureSetClosed(IReadOnlySet<string> planFeatureNames)
    {
        var errors = new List<string>();

        foreach (var name in planFeatureNames)
        {
            var missing = FeatureDependencies.GetPrerequisites(name)
                .Where(p => !planFeatureNames.Contains(p))
                .ToList();

            if (missing.Count > 0)
                errors.Add($"'{name}' requires {string.Join(", ", missing)} to also be included in this plan.");
        }

        return errors;
    }
}

public readonly record struct FeatureRuntimeState(bool IsEnabled, bool LockedBySuperAdmin);

public sealed record FeatureCascadeResult(bool IsBlocked, IReadOnlyList<string> AffectedFeatureNames, IReadOnlyList<string> LockedConflicts)
{
    public static FeatureCascadeResult Ok(IReadOnlyList<string> affected) => new(false, affected, []);
    public static FeatureCascadeResult Conflict(IReadOnlyList<string> lockedNames) => new(true, [], lockedNames);
}
