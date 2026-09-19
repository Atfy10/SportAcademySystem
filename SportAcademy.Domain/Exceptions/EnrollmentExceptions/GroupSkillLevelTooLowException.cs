namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class GroupSkillLevelTooLowException : Exception
    {
        public GroupSkillLevelTooLowException(int traineeId, int traineeGroupId)
            : base($"Trainee group {traineeGroupId}'s skill level is below trainee {traineeId}'s own " +
                   $"skill level for this sport. Choose a group at or above the trainee's own skill level.")
        {
        }
    }
}
