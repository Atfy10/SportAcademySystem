namespace SportAcademy.Domain.Entities.Tenants;

public class SubscriptionPlanFeature
{
    public Guid FeatureId { get; set; }
    public int SubscriptionPlanId { get; set; }

    public Feature Feature { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
}
