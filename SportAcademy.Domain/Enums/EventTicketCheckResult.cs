namespace SportAcademy.Domain.Enums;

// What the door scanner (and the guest's own ticket page) says about a ticket.
public enum EventTicketCheckResult
{
    // Good to go in - staff can admit it now.
    Valid,
    // Staff just let this ticket in.
    Admitted,
    // This ticket was already used to get in.
    AlreadyUsed,
    // Too early: entry opens EventEntryRules.OpensBefore before the start.
    NotYetOpen,
    // The event is over - every ticket is terminated.
    Ended,
    // The event was cancelled - every ticket is terminated.
    Cancelled,
    // Unknown, revoked or re-issued code, or the academy can't take entries right now.
    Invalid,
}
