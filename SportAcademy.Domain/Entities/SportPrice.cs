using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class SportPrice : ITenantScoped
    {
        public int SportId { get; set; }
        public int BranchId { get; set; }
        public int SubsTypeId { get; set; }
        public decimal Price { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Branch Branch { get; set; } = null!;
        public virtual SportBranch SportBranch { get; set; } = null!;
        public virtual SportSubscriptionType SportSubscriptionType { get; set; } = null!;
        public virtual ICollection<SubscriptionDetails> SubscriptionsDetails { get; set; } = null!;
    }
}
