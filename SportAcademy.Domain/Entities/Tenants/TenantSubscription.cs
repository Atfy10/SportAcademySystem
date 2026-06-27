using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Tenants;

public class TenantSubscription
{
    public Guid TenantId { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public bool IsTrial { get; set; }
    public bool AutoRenew { get; set; }
    public int SubscriptionPlanId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public SubscriptionPlan Plan { get; set; } = null!;
}
