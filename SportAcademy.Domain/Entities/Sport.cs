using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Sport : ITenantScoped
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public SportCategory Category { get; set; }
        public bool IsRequireHealthTest { get; set; } = true;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Coach> Coaches { get; set; } = [];
        public virtual ICollection<SportSubscriptionType> SubscriptionTypes { get; set; } = [];
        public virtual ICollection<SportBranch> Branches { get; set; } = [];
        public virtual ICollection<SportTrainee> Trainees { get; set; } = [];
	}
}
