using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class TraineeGroup : ITenantScoped, IAuditableEntity, IBranchScoped
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public SkillLevel SkillLevel { get; set; }
        public TraineeGroupType Type { get; set; } = TraineeGroupType.Public;
        public int MaximumCapacity { get; set; } = 15;
        public int DurationInMinutes { get; set; } = 55;
        public TraineeGroupGender Gender { get; set; }
        public int BranchId { get; set; }
        public int CoachId { get; set; }
        public bool IsActive { get; set; } = true;
        /// <summary>Staff-provided reason shown while the group is paused (IsActive = false). Null while active.</summary>
        public string? InactiveReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach Coach { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
        public virtual ICollection<GroupSchedule> GroupSchedules { get; set; } = [];
        public virtual ICollection<TraineeGroupTranslation> Translations { get; set; } = [];
    }
}
