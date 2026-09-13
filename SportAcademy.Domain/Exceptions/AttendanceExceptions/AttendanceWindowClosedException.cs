namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    public class AttendanceWindowClosedException : Exception
    {
        public AttendanceWindowClosedException(int sessionOccurrenceId)
            : base($"Session {sessionOccurrenceId}'s attendance can only be recorded from when the session starts until 120 minutes after it ends.")
        {
        }
    }
}
