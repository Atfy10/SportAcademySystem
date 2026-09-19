namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    // Thrown when attendance is attempted against a session occurrence that's no longer
    // Scheduled - either the completion sweep (SessionOccurrenceCompletionService) already
    // closed it out once its attendance window passed, or staff marked it
    // Completed/Canceled/CancelledTemporary by hand. Distinct from AttendanceWindowClosedException:
    // this can reject a session whose time window technically hasn't closed yet (an early manual
    // Complete/Cancel), where the time check alone wouldn't catch it.
    public class SessionNotScheduledException : Exception
    {
        public SessionNotScheduledException(int sessionOccurrenceId, string status)
            : base($"Session {sessionOccurrenceId} is {status}, not Scheduled - attendance can no longer be marked for it.")
        {
        }
    }
}
