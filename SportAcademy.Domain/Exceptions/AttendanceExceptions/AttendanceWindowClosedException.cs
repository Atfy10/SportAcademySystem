namespace SportAcademy.Domain.Exceptions.AttendanceExceptions
{
    public class AttendanceWindowClosedException : Exception
    {
        // Single source of truth for the grace period, referenced by both
        // CreateAttendanceCommandHandler and BulkCreateAttendanceCommandHandler's window checks
        // - keeps the enforced value and this message from silently drifting apart.
        public const int GraceMinutesAfterEnd = 90;

        public AttendanceWindowClosedException(int sessionOccurrenceId)
            : base($"Session {sessionOccurrenceId}'s attendance can only be recorded from when the session starts until {GraceMinutesAfterEnd} minutes after it ends.")
        {
        }
    }
}
