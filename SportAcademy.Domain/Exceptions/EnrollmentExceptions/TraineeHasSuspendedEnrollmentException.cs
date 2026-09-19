namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class TraineeHasSuspendedEnrollmentException : Exception
    {
        public TraineeHasSuspendedEnrollmentException(int traineeId, int sportId, int suspendedEnrollmentId)
            : base($"Trainee {traineeId} has a suspended enrollment (#{suspendedEnrollmentId}) for sport {sportId}. " +
                   "Reactivate it to resume training in that group - a trainee can only be enrolled in one " +
                   "group per sport, and a suspended enrollment still holds that spot.")
        {
        }
    }
}
