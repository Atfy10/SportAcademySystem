namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class TraineeGenderMismatchException : Exception
    {
        public TraineeGenderMismatchException(int traineeId, int traineeGroupId)
            : base($"Trainee {traineeId}'s gender does not match trainee group {traineeGroupId}'s gender " +
                   "policy. A Male or Female group only accepts trainees of that gender - use a Mixed group instead.")
        {
        }
    }
}
