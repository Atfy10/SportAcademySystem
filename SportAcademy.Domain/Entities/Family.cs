using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class Family : ITenantScoped
    {
        public int Id { get; set; }
        public int FamilyCode { get; set; }
        public int LastMemberNumber { get; set; }
        public string? Name { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public ICollection<Trainee> Members { get; } = [];
    }
}
