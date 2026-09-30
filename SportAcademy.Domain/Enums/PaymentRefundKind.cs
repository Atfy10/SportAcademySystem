namespace SportAcademy.Domain.Enums
{
    // A Refund gives back part (or all) of a payment by agreement; a Void reverses whatever is
    // still unrefunded because the payment itself was recorded in error. Both reopen the
    // invoice balance they came from - the difference is intent, and it's kept for the audit trail.
    public enum PaymentRefundKind
    {
        Refund = 0,
        Void = 1,
    }
}
