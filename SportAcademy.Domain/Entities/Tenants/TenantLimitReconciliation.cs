namespace SportAcademy.Domain.Entities.Tenants;

// The open (or completed) "you're over your new limit, choose what survives" task created by
// LimitReconciliationService whenever a plan/limit/override change pushes a tenant over a
// selection-requiring limit (LimitedResources.RequiresSelection). Exactly one row may be open
// (CompletedAt == null) per tenant at a time - see LimitReconciliationService.EvaluateAsync.
//
// Deliberately NOT ITenantScoped, for the same reason as TenantFeature/TenantLimitOverride: the
// platform console and the deadline background sweep both need to read/write this across every
// tenant with no ambient tenant context.
public class TenantLimitReconciliation
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>The plan the tenant was on at the moment this reconciliation opened - context
    /// for the console, not itself load-bearing (the actual ceiling is looked up live via
    /// IEffectiveLimitService, which could have changed again since).</summary>
    public int TriggeredByPlanId { get; set; }

    public DateTime OpenedAt { get; set; }
    public DateTime DeadlineAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedByUserId { get; set; }

    /// <summary>
    /// Snapshot, as JSON, of which resources were over their cap and by how much at the moment
    /// this reconciliation opened - a Dictionary&lt;string, int&gt; of resourceKey -> used count.
    /// Snapshotted rather than recomputed on every read so the wizard shows the operator the
    /// same numbers throughout the whole window, and so the record reflects what was actually
    /// demanded of them even if the tenant's usage or plan changes again before they act.
    /// </summary>
    public string RequiredResourcesJson { get; set; } = null!;

    public Tenant Tenant { get; set; } = null!;
}
