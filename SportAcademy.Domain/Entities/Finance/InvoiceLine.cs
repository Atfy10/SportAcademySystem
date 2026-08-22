using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Finance;

// The extension seam for future billable things: a discount, a late fee, tax, a manual
// adjustment - each is a new InvoiceLine.Type value on an existing invoice, not a new table.
// Price/Description are snapshotted at creation time so that later editing the source price
// (e.g. SportPrice.Price) can never silently rewrite historical financial documents.
public class InvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public InvoiceLineType Type { get; set; }
    public required string Description { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }

    // What this line billed, if anything - nullable because not every future line type will
    // point at a subscription (e.g. a manual adjustment or a late fee might not).
    public int? SubscriptionDetailsId { get; set; }

    public Invoice Invoice { get; set; } = null!;
    public SubscriptionDetails? SubscriptionDetails { get; set; }
}
