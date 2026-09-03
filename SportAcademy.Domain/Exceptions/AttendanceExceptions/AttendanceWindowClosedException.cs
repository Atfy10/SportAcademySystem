namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    public class AttendanceWindowClosedException : Exception
    {
        public AttendanceWindowClosedException(int sessionOccurrenceId)
            : base($"Session {sessionOccurrenceId}'s attendance window closed 15 minutes after it ended - attendance can no longer be marked for it.")
        {
        }
    }
}
