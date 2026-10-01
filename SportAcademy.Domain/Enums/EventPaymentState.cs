namespace SportAcademy.Domain.Enums;

// Where an event's bill stands, from its invoice - separate from EventStatus (an upcoming event
// can be partially paid, a completed one can still owe money).
public enum EventPaymentState
{
    Paid,
    PartiallyPaid,
    Overdue,
    Unpaid,
}
