using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Coach
    {
        public SkillLevel SkillLevel { get; set; }
        public int EmployeeId { get; set; }
        public int SportId { get; set; }
        public DateOnly BirthDate { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Sport Sport { get; set; } = null!;
        public virtual ICollection<TraineeGroup> TraineeGroups { get; set; } = [];
    }
}
