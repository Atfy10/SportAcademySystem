namespace SportAcademy.Domain.Enums;

// Where a subscription's bill stands - separate from its lifecycle status (a subscription can be
// Active and Overdue at the same time; overdue balances warn, they don't block).
public enum SubscriptionPaymentState
{
    Paid,
    PartiallyPaid,
    Overdue,
    Unpaid,
}
