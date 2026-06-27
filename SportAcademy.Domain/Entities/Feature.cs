using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class Feature
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string DisplayName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<TenantFeature> TenantFeatures { get; set; } = [];
        public virtual ICollection<SubscriptionPlanFeature> SubscriptionPlans { get; set; } = [];
    }
}
