using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Entities.Translations;

namespace SportAcademy.Domain.Entities
{
    public class Family : ITenantScoped, ISoftDeletable
    {
        public int Id { get; set; }
        public int FamilyCode { get; set; }
        public int LastMemberNumber { get; set; }
        public string? Name { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }

        // Soft-deleted, not removed: a trainee is soft-deleted too and keeps its (required)
        // FamilyId, so a family whose members were all deleted is still referenced by them.
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public ICollection<Trainee> Members { get; } = [];
        public ICollection<FamilyTranslation> Translations { get; set; } = [];
    }
}
