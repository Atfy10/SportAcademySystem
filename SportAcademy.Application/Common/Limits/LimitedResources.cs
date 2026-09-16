namespace SportAcademy.Application.Common.Limits;

// Code-defined registry of which resources a plan can put a numeric cap on - the quantitative
// twin of FeatureDependencies (also code-defined, not DB-backed). The *policy* of what's
// limitable, and which limited resources participate in the forced-selection wizard, lives here
// where it's reviewable in a diff; only the *numbers* (PlanLimit.MaxCount,
// TenantLimitOverride.MaxCount) live in the database.
public static class LimitedResources
{
    public const string Branches = "branches";
    public const string Users = "users";
    public const string Sports = "sports";
    public const string Trainees = "trainees";

    public static readonly IReadOnlyList<string> All = [Branches, Users, Sports, Trainees];

    /// <summary>
    /// Which resources participate in the forced-selection wizard when a downgrade puts a
    /// tenant over its new cap. Trainees deliberately do not: you cannot ask an operator to
    /// hand-pick 300 of 450 enrolled children, and freezing trainees mid-selection punishes
    /// paying customers who had no say in the downgrade. Trainees are grandfathered instead -
    /// existing ones are untouched, only new registration is blocked while over cap.
    /// </summary>
    public static bool RequiresSelection(string resourceKey) =>
        resourceKey is Branches or Users or Sports;

    /// <summary>
    /// True for any key this registry recognizes. A PlanLimit/TenantLimitOverride row with an
    /// unrecognized ResourceKey (e.g. left behind after a rollback, or a typo from a future
    /// version) is ignored by IEffectiveLimitService rather than thrown on - see its own comment.
    /// </summary>
    public static bool IsKnown(string resourceKey) => All.Contains(resourceKey);
}
