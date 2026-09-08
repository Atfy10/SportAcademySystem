namespace SportAcademy.Application.Common.Features;

// Single source of truth for "requires" edges between features - same static-map shape as
// FeatureCategories.cs, deliberately not DB-backed (no tenant/plan currently needs to edit
// this graph; it's a product-level invariant, not per-tenant data). Consulted by
// FeatureDependencyPolicy, which is the only place that actually enforces these edges - this
// class just exposes the graph and its transitive closure.
//
// Keys and values are Feature.Name slugs (see AppDataSeeder.FeatureCatalog). An edge
// ["a"] = ["b"] means "a requires b" - b must be enabled for a to be enabled.
public static class FeatureDependencies
{
    // Foundational platform capabilities, not optional add-ons: nothing anywhere gates a
    // command on these (there's no code path where "user-management disabled" is a coherent
    // state - it would just lock a tenant out of managing their own account with no obvious way
    // back in). Two different levels of protection apply depending on who's asking:
    //  - Tenant Owner self-service: never allowed, full stop (ValidateDisable/ValidateEndState).
    //  - SuperAdmin (ToggleFeatureCommand): allowed, but only with explicit confirmation
    //    (ConfirmProtectedDisable) - a SuperAdmin can always re-enable it afterward, so this is a
    //    "are you sure" gate, not a hard block.
    // Plan-driven reconciliation (PlanFeatureReconciler) is neither of those - it's an automatic
    // process with no human acknowledging a warning, so it treats this set the same as tenant
    // self-service: never disables it.
    private static readonly HashSet<string> Protected =
    [
        "user-management",
        "role-management",
        "tenant-settings",
        "profile-mgmt",
        "system-settings",
    ];

    public static bool IsProtected(string featureName) => Protected.Contains(featureName);

    private static readonly Dictionary<string, string[]> Requires = new()
    {
        ["attendance-tracking"] = ["session-management", "enrollment-management"],
        ["session-management"] = ["group-management", "schedule-management"],
        ["enrollment-management"] = ["trainee-management", "subscription-plan"],
        ["group-management"] = ["branch-management", "sport-management"],
        ["coach-management"] = ["branch-management"],
        ["pricing-management"] = ["sport-management", "branch-management"],
        ["discount-offers"] = ["subscription-plan"],
        ["family-management"] = ["trainee-management"],
    };

    // Built once from Requires by inverting every edge - not hand-maintained separately, so it
    // can never drift from the forward map above.
    private static readonly Dictionary<string, string[]> Dependents = Requires
        .SelectMany(kvp => kvp.Value.Select(dep => (Feature: kvp.Key, DependsOn: dep)))
        .GroupBy(x => x.DependsOn, x => x.Feature)
        .ToDictionary(g => g.Key, g => g.ToArray());

    public static IReadOnlyList<string> GetPrerequisites(string featureName) =>
        Requires.GetValueOrDefault(featureName, []);

    public static IReadOnlyList<string> GetDependents(string featureName) =>
        Dependents.GetValueOrDefault(featureName, []);

    public static HashSet<string> GetTransitivePrerequisites(string featureName) =>
        Walk(featureName, Requires);

    public static HashSet<string> GetTransitiveDependents(string featureName) =>
        Walk(featureName, Dependents);

    private static HashSet<string> Walk(string start, Dictionary<string, string[]> edges)
    {
        var visited = new HashSet<string>();
        var queue = new Queue<string>(edges.GetValueOrDefault(start, []));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current))
                continue;

            foreach (var next in edges.GetValueOrDefault(current, []))
                queue.Enqueue(next);
        }

        return visited;
    }
}
