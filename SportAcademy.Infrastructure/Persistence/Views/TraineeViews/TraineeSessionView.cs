using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Persistence.Views.TraineeViews
{
    public class TraineeSessionView
    {
        public int Id { get; set; }

        public DateOnly EnrollmentDate { get; set; }

        public DateOnly ExpiryDate { get; set; }

        public int SessionAllowed { get; set; }

        public int SessionRemaining { get; set; }

        public int TraineeGroupId { get; set; }

        public SkillLevel SkillLevel { get; set; }

        public int MaximumCapacity { get; set; }

        public int DurationInMinutes { get; set; }

        public Gender Gender { get; set; }

        public string BranchName { get; set; } = null!;

        public string CoachName { get; set; } = null!;
    }
}
