using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SubscriptionType : ITenantScoped
    {
        public int Id { get; set; }
        public SubType Name { get; set; }
        public int DaysPerMonth { get; set; }
        public int NumberOfMonths { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsOffer { get; set; } = false;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<SportSubscriptionType> Sports { get; set; } = [];
    }
}
