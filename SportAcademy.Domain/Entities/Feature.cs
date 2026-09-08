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
        // False for a catalog entry that exists (sold on a plan, shown as a toggle) but has no
        // code anywhere that actually enforces or delivers it yet - see FeatureCatalog's own
        // comment in AppDataSeeder for the current list and why each one is in that state. The
        // UI renders these as a disabled switch with a "not available yet" badge instead of a
        // toggle that silently does nothing either way.
        public bool IsImplemented { get; set; } = true;

        // Navigation properties
        public virtual ICollection<TenantFeature> TenantFeatures { get; set; } = [];
        public virtual ICollection<SubscriptionPlanFeature> SubscriptionPlans { get; set; } = [];
    }
}
