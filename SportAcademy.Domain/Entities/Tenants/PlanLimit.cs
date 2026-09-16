namespace SportAcademy.Domain.Entities.Tenants;

// The plan's quantitative grant - the numeric twin of SubscriptionPlanFeature, which already
// carries the plan's qualitative grant. Kept as a separate table rather than folded into
// SubscriptionPlan itself so a new limited resource never needs a schema change to the plan
// table - see LimitedResources.
public class PlanLimit
{
    public int Id { get; set; }
    public int SubscriptionPlanId { get; set; }

    // Free-form key from LimitedResources, not an enum - adding a 5th limited resource must not
    // require a migration. An unrecognized key found in the database (e.g. after a rollback) is
    // ignored by the resolver rather than thrown on - see IEffectiveLimitService.
    public string ResourceKey { get; set; } = null!;

    // null = unlimited. Nullable rather than int.MaxValue: "unlimited" is a product decision the
    // console renders as a switch, not a number someone has to read as one - e.g. Enterprise's
    // "unlimited sports".
    public int? MaxCount { get; set; }

    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}
