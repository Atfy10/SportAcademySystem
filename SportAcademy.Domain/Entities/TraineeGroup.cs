using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class TraineeGroup : IAuditableEntity
    {
        private TraineeGroup() { }

        private TraineeGroup(string? name, SkillLevel skillLevel, int maximumCapacity, int durationInMinutes, Gender gender, int branchId, int coachId)
        {
            Name = name ?? "Trainee Group";
            SkillLevel = skillLevel;
            MaximumCapacity = maximumCapacity;
            DurationInMinutes = durationInMinutes;
            Gender = gender;
            BranchId = branchId;
            CoachId = coachId;
            CreatedAt = DateTime.UtcNow;
        }

        public int Id { get; private set; }
        public string Name { get; private set; } = null!;
        public SkillLevel SkillLevel { get; private set; }
        public int MaximumCapacity { get; private set; } = 15;
        public int DurationInMinutes { get; private set; } = 55;
        public Gender Gender { get; private set; }
        public int BranchId { get; private set; }
        public int CoachId { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual Branch Branch { get; private set; } = null!;
        public virtual Coach Coach { get; private set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; private set; } = [];
        public virtual ICollection<GroupSchedule> GroupSchedules { get; private set; } = [];

        public static TraineeGroup Create(string? name, SkillLevel skillLevel, int maximumCapacity, int durationInMinutes, Gender gender, int branchId, int coachId)
        {
            return new TraineeGroup(name, skillLevel, maximumCapacity, durationInMinutes, gender, branchId, coachId);
        }

        public void Update(SkillLevel skillLevel, int maximumCapacity, int durationInMinutes, Gender gender, int branchId, int coachId)
        {
            SkillLevel = skillLevel;
            MaximumCapacity = maximumCapacity;
            DurationInMinutes = durationInMinutes;
            Gender = gender;
            BranchId = branchId;
            CoachId = coachId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetName(string name)
        {
            Name = name;
        }
    }
}
