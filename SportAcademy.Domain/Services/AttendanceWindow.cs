namespace SportAcademy.Domain.Services
{
    /// <summary>
    /// When attendance can be recorded for a session: from the moment the session starts until
    /// midnight (12:00 AM) at the end of that same calendar day.
    /// </summary>
    /// <remarks>
    /// The single definition of the rule - CreateAttendanceCommandHandler,
    /// BulkCreateAttendanceCommandHandler and SessionOccurrenceCompletionService (which closes a
    /// session once its window is over) all ask this class, so they cannot drift apart.
    /// Every value here is the tenant's own wall-clock time (session generation writes
    /// StartDateTime that way and ITenantClock resolves "now" the same way), never UTC. A session
    /// that runs past midnight still closes at that midnight.
    /// </remarks>
    public static class AttendanceWindow
    {
        /// <summary>The instant the window closes: 00:00 of the day after the session starts.
        /// Attendance at exactly this instant is no longer allowed.</summary>
        public static DateTime ClosesAt(DateTime sessionStart) => sessionStart.Date.AddDays(1);

        /// <summary>True from <paramref name="sessionStart"/> up to (not including) midnight
        /// that day. Not before the session starts - there is nothing to attend yet.</summary>
        public static bool IsOpen(DateTime tenantNow, DateTime sessionStart) =>
            tenantNow >= sessionStart && tenantNow < ClosesAt(sessionStart);
    }
}
