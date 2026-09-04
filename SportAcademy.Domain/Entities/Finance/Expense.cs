using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities.Finance;

// What the academy spent, independent of the Invoice/Payment ledger (which tracks what it is
// owed) - additive to that ledger, not a replacement or a source it draws from.
public class Expense : ITenantScoped, IBranchScoped, IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public int ExpenseCategoryId { get; set; }
    public int BranchId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KWD";
    public DateOnly ExpenseDate { get; set; }
    public int? PaymentTypeId { get; set; }
    public string? Notes { get; set; }
    public Guid RecordedByUserId { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public ExpenseCategory ExpenseCategory { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public PaymentType? PaymentType { get; set; }
}
