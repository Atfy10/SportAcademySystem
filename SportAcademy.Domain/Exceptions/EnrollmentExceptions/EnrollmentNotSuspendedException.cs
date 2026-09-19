namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class EnrollmentNotSuspendedException : Exception
    {
        public EnrollmentNotSuspendedException(int enrollmentId)
            : base($"Enrollment {enrollmentId} isn't suspended, so there's nothing to reactivate.")
        {
        }
    }
}
