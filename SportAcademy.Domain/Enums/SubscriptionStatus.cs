namespace SportAcademy.Domain.Enums;

public enum SubscriptionStatus
{
    None,
    Active,
    Suspended,
    Expired,

    // Display-only, never stored: an Active subscription whose StartDate is still in the future.
    // Derived on read (SubscriptionBilling.EffectiveStatus) so it flips to Active on its start
    // date by itself, with no job having to rewrite the row.
    Upcoming
}
