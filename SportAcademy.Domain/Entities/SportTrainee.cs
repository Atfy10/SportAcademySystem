using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SportTrainee : ITenantScoped
    {
        public int SportId { get; set; }
        public int TraineeId { get; set; }
        public SkillLevel SkillLevel { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;

    }
}
