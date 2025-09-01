using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SportTrainee
    {
        public int SportId { get; set; }
        public int TraineeId { get; set; }
        public SkillLevel SkillLevel { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;

    }
}
