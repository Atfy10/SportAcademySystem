using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Finance;

// What the academy is owed. Created whenever something is billed (today: subscription
// creation; later: any other charge type, expressed as a new InvoiceLine, not a new table -
// see InvoiceLine). AmountPaid is maintained by IFinanceLedgerService, the only code allowed
// to mutate it or Status - never derive/recompute it ad hoc in a query.
public class Invoice : ITenantScoped, IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public required string InvoiceNumber { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Issued;
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }

    // Nullable so a future payer type (e.g. a family/company account) fits without a schema
    // change - today it is always set.
    public int? TraineeId { get; set; }
    public int BranchId { get; set; }
    public string Currency { get; set; } = "KWD";

    public decimal SubTotal { get; set; }
    public decimal DiscountTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal AmountPaid { get; set; }
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Trainee? Trainee { get; set; }
    public Branch Branch { get; set; } = null!;
    public ICollection<InvoiceLine> Lines { get; set; } = [];
    public ICollection<PaymentAllocation> Allocations { get; set; } = [];

    public decimal Outstanding => GrandTotal - AmountPaid;
}
