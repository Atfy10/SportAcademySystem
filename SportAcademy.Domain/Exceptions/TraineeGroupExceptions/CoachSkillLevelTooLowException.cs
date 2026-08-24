using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class CoachSkillLevelTooLowException : Exception
    {
        public CoachSkillLevelTooLowException(int coachId, SkillLevel coachSkillLevel, SkillLevel requiredSkillLevel)
            : base($"Coach {coachId}'s skill level ({coachSkillLevel}) is below the group's required " +
                   $"skill level ({requiredSkillLevel}). A coach can only lead a group at or below their own skill level.")
        {
        }
    }
}
