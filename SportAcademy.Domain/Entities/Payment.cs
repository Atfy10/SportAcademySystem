using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    // Money actually received. Not tied to a single subscription - what it settles is
    // expressed entirely through Allocations (see PaymentAllocation), which is what lets one
    // payment cover several invoices (e.g. a family paying for multiple trainees at once) or
    // an invoice be settled by several payments (instalments).
    public class Payment : ITenantScoped, IAuditableEntity
    {
        public required string PaymentNumber { get; set; }
        public int PaymentTypeId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
        public DateTime PaidDate { get; set; } = DateTime.Now;
        public int BranchId { get; set; }
        public string Currency { get; set; } = "KWD";
        public decimal Amount { get; set; }
        public decimal RefundedAmount { get; set; }
        public Guid? RecordedByUserId { get; set; }

        // Cheque number / gateway transaction id - the seam a future payment-gateway
        // integration hangs off, alongside Method.
        public string? Reference { get; set; }
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Branch Branch { get; set; } = null!;
        public virtual PaymentType PaymentType { get; set; } = null!;
        public ICollection<PaymentAllocation> Allocations { get; set; } = [];
    }
}
