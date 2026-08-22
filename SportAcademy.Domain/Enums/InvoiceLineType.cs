namespace SportAcademy.Domain.Enums
{
    // Every future billable thing is a new line type on this enum, not a new table - see
    // Invoice/InvoiceLine remarks.
    public enum InvoiceLineType
    {
        SubscriptionFee = 0,
        Discount = 1,
        LateFee = 2,
        Adjustment = 3,
        Tax = 4,
    }
}
