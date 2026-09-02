using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class TraineeCareerEvent : ITenantScoped
    {
        public int Id { get; set; }
        public int TraineeId { get; set; }
        public TraineeCareerEventType EventType { get; set; }

        public int? SportId { get; set; }
        public int? TraineeGroupId { get; set; }
        public int? CoachId { get; set; }
        public int? EnrollmentId { get; set; }
        public SkillLevel? SkillLevel { get; set; }

        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Reason { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Properties
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual Sport? Sport { get; set; }
        public virtual TraineeGroup? TraineeGroup { get; set; }
        public virtual Coach? Coach { get; set; }
        public virtual Enrollment? Enrollment { get; set; }
    }
}
