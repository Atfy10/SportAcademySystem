namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class TraineeAlreadyEnrolledInSportException : Exception
    {
        public TraineeAlreadyEnrolledInSportException(int traineeId, int sportId)
            : base($"Trainee {traineeId} already has an active enrollment for sport {sportId}. " +
                   "Use Change Group to move them to a different group, or Renew to extend their " +
                   "current subscription - a trainee can only be enrolled in one group per sport.")
        {
        }
    }
}
