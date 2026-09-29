using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Finance;

// One row per refund or void applied to a Payment - the history behind Payment.RefundedAmount
// (which stays as the denormalized running total). RefundedAt is what reports use to place
// money going back out in the month it actually left, not the month it originally came in.
// Written only by IFinanceLedgerService.
public class PaymentRefund : ITenantScoped, IAuditableEntity
{
    public int Id { get; set; }
    public required string PaymentNumber { get; set; }
    public PaymentRefundKind Kind { get; set; }
    public decimal Amount { get; set; }
    public required string Reason { get; set; }
    public DateTime RefundedAt { get; set; } = DateTime.UtcNow;
    public Guid? RefundedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Payment Payment { get; set; } = null!;
}
