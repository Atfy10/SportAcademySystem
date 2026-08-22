namespace SportAcademy.Domain.Entities.Finance;

// The many-to-many join between what was received (Payment) and what it settles (Invoice).
// This single table is what buys partial payments, instalments, one payment covering several
// invoices, and prepayment/credit - all without further schema change. Never inserted/updated
// directly outside IFinanceLedgerService, which is what keeps
// "Invoice.AmountPaid == sum of its allocations" true.
public class PaymentAllocation
{
    public int Id { get; set; }
    public required string PaymentNumber { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }

    public Payment Payment { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
}
