using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Coach : ISoftDeletable
    {
        private Coach() { }

        private Coach(int employeeId, int sportId, SkillLevel skillLevel)
        {
            EmployeeId = employeeId;
            SportId = sportId;
            SkillLevel = skillLevel;
            Rate = 2;
        }

        public SkillLevel SkillLevel { get; private set; }
        public int Rate { get; private set; }
        public int EmployeeId { get; private set; }
        public int SportId { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public virtual Employee Employee { get; private set; } = null!;
        public virtual Sport Sport { get; private set; } = null!;
        public virtual ICollection<TraineeGroup> TraineeGroups { get; private set; } = [];

        public static Coach Create(int employeeId, int sportId, SkillLevel skillLevel)
        {
            return new Coach(employeeId, sportId, skillLevel);
        }

        public void MarkDeleted(string? deletedBy = null)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy ?? "System";
        }
    }
}
