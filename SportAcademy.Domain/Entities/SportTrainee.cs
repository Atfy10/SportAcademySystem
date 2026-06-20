using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SportTrainee
    {
        public int SportId { get; private set; }
        public int TraineeId { get; private set; }
        public SkillLevel SkillLevel { get; private set; }

        public virtual Sport Sport { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;

        private SportTrainee() { }

        private SportTrainee(int sportId, int traineeId, SkillLevel skillLevel)
        {
            SportId = sportId;
            TraineeId = traineeId;
            SkillLevel = skillLevel;
        }

        public static SportTrainee Create(int sportId, int traineeId, SkillLevel skillLevel)
            => new(sportId, traineeId, skillLevel);

        public static SportTrainee CreateWithoutTrainee(int sportId, SkillLevel skillLevel)
            => new SportTrainee { SportId = sportId, SkillLevel = skillLevel };

        public void UpdateSkillLevel(SkillLevel skillLevel)
        {
            SkillLevel = skillLevel;
        }
    }
}
