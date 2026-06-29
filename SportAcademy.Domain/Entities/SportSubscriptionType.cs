using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class SportSubscriptionType : ITenantScoped
    {
        public int SportId { get; set; }
        public int SubscriptionTypeId { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual SubscriptionType SubscriptionType { get; set; } = null!;
        public virtual ICollection<SportPrice> SportPrices { get; set; } = null!;
    }
}
