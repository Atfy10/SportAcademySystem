namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    // Thrown when an action that would discard a Suspended enrollment (currently: delete) is
    // attempted. Suspended enrollments can still hold unspent sessions - there is deliberately
    // no way to remove one outright yet; reactivate it, or wait for a dedicated close flow
    // (still to be built) that decides what happens to those remaining sessions first.
    public class EnrollmentSuspendedException : Exception
    {
        public EnrollmentSuspendedException(int enrollmentId)
            : base($"Enrollment {enrollmentId} is suspended and can't be deleted - reactivate it first, " +
                   "or leave it suspended. There is no way to discard a suspended enrollment's remaining " +
                   "sessions yet.")
        {
        }
    }
}
