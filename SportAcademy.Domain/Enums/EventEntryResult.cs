namespace SportAcademy.Domain.Enums;

// What the entry page tells the person at the door after a scan.
public enum EventEntryResult
{
    // Let in now - this scan took a place.
    Admitted,
    // This phone was already let in earlier; no new place taken.
    AlreadyAdmitted,
    // Every place is taken - entry refused.
    Full,
    // Too early: entry opens EventEntryRules.OpensBefore before the start.
    NotYetOpen,
    Ended,
    Cancelled,
    // Unknown or replaced code, or the academy can't take entries right now.
    Invalid,
}
