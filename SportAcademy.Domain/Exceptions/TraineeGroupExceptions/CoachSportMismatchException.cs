namespace SportAcademy.Domain.Exceptions.TraineeGroupExceptions
{
    public class CoachSportMismatchException : Exception
    {
        public CoachSportMismatchException(int oldCoachId, int newCoachId)
            : base($"Coach {newCoachId} does not teach the same sport as coach {oldCoachId}. " +
                   "Reassigning a group to a coach of a different sport would change the sport of every " +
                   "trainee already enrolled in it - create a new group instead.")
        {
        }
    }
}
