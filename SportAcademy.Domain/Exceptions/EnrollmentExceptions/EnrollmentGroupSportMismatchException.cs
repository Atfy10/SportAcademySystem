namespace SportAcademy.Domain.Exceptions.EnrollmentExceptions
{
    public class EnrollmentGroupSportMismatchException : Exception
    {
        public EnrollmentGroupSportMismatchException(int enrollmentId, int newTraineeGroupId)
            : base($"Trainee group {newTraineeGroupId} teaches a different sport than enrollment " +
                   $"{enrollmentId}'s current group. Changing groups only moves a trainee between " +
                   "groups for the same sport - switching sports needs a new subscription and enrollment.")
        {
        }
    }
}
