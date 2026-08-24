using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class PaymentType : ITenantScoped, IAuditableEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public bool IsActive { get; set; } = true;
        // The type the "mark enrollment as paid" quick action records against - exactly one
        // per tenant. Enforced by the Create/Update handlers (clearing any previous default),
        // not by a DB constraint.
        public bool IsDefault { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual ICollection<Payment> Payments { get; set; } = [];
    }
}
