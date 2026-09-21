namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    public class AttendanceWindowClosedException : Exception
    {
        // The rule itself lives in AttendanceWindow (Domain/Services) - this is only its message.
        public AttendanceWindowClosedException(int sessionOccurrenceId)
            : base($"Session {sessionOccurrenceId}'s attendance can only be recorded from when the session starts until midnight (12:00 AM) at the end of the same day.")
        {
        }
    }
}
