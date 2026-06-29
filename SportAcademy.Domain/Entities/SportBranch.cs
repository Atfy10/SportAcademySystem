using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class SportBranch : ITenantScoped
    {
        public int SportId { get; set; }
        public int BranchId { get; set; }
        public bool IsAvailable { get; set; } = true;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
    }
}
