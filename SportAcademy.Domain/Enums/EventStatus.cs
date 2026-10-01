namespace SportAcademy.Domain.Enums;

// Derived on read from the event's dates and IsCancelled - never stored.
public enum EventStatus
{
    Upcoming,
    Ongoing,
    Completed,
    Cancelled,
}
