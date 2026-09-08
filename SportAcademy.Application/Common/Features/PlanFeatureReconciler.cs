using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Application.Common.Features;

// Single source of truth for reconciling a tenant's TenantFeature rows against a plan's feature
// set - shared by ChangeTenantPlanCommandHandler (a tenant moves to a different plan) and
// UpdatePlanFeaturesCommandHandler (a plan's own feature set changes, reconciled retroactively
// against every tenant currently on it). Symmetric in both callers: anything the plan grants
// that isn't already enabled gets enabled, anything it no longer grants that's currently enabled
// gets disabled. A SuperAdmin-locked feature (TenantFeature.LockedBySuperAdmin) is never touched
// either way - a forced decision overrides plan membership entirely.
public static class PlanFeatureReconciler
{
    public static Dictionary<Guid, bool> ComputeUpdates(
        List<TenantFeature> currentFeatures, List<Guid> planFeatureIds)
    {
        var byFeature = currentFeatures.ToDictionary(f => f.FeatureId);
        var planSet = planFeatureIds.ToHashSet();
        var updates = new Dictionary<Guid, bool>();

        foreach (var featureId in planSet)
        {
            if (byFeature.TryGetValue(featureId, out var existing))
            {
                if (!existing.LockedBySuperAdmin && !existing.IsEnabled)
                    updates[featureId] = true;
            }
            else
            {
                // No row yet - BulkUpdateFeaturesAsync inserts a new enabled row for this case.
                updates[featureId] = true;
            }
        }

        foreach (var existing in currentFeatures)
        {
            if (!existing.LockedBySuperAdmin && existing.IsEnabled && !planSet.Contains(existing.FeatureId))
                updates[existing.FeatureId] = false;
        }

        return updates;
    }
}
