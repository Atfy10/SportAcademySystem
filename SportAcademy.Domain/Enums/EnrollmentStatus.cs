namespace SportAcademy.Domain.Enums
{
    // A trainee's own recorded relationship to a group - orthogonal to whether the backing
    // subscription has run out (that's ExpiryDate, checked live at display time, never stored
    // here - "Expired" is a display overlay on top of Active/Suspended, not a fourth stored
    // value, so there's nothing to keep in sync when time passes).
    public enum EnrollmentStatus
    {
        // Currently training - counted for attendance rosters, session-occurrence "enrolled"
        // totals, and the dashboard's "active enrollments" stat.
        Active,

        // Staff-paused: still holds the group's capacity slot (a trainee doesn't lose their
        // seat by pausing) and still blocks a second enrollment for the same sport, but drops
        // off attendance rosters. Reversible via Reactivate. EndDate stays null.
        Suspended,

        // Permanently closed - either the trainee lapsed past the grace window
        // (EnrollmentLapseService) or was manually closed. EndDate is always set alongside this.
        // Terminal: never reversible back to Active/Suspended.
        Ended
    }
}
