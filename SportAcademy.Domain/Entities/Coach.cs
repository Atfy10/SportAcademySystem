using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Coach : ISoftDeletable
    {
        public SkillLevel SkillLevel { get; set; }
        public int EmployeeId { get; set; }
        public int SportId { get; set; }
        public DateOnly BirthDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Sport Sport { get; set; } = null!;
        public virtual ICollection<TraineeGroup> TraineeGroups { get; set; } = [];
    }
}
