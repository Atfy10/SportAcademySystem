namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class TraineeSkillLevelTooLowException : Exception
    {
        public TraineeSkillLevelTooLowException(int traineeId, int traineeGroupId)
            : base($"Trainee {traineeId}'s skill level for this sport is below trainee group " +
                   $"{traineeGroupId}'s required skill level. Choose a group at or below the trainee's own skill level.")
        {
        }
    }
}
