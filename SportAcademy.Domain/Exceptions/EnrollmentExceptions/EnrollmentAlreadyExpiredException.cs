namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class EnrollmentAlreadyExpiredException : Exception
    {
        public EnrollmentAlreadyExpiredException(int enrollmentId)
            : base($"Enrollment {enrollmentId} has already expired and can no longer be suspended - " +
                   "suspending pauses a live enrollment, it isn't a way to close an expired one.")
        {
        }
    }
}
