using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Finance;

// A staff salary payment, gated by an approval workflow (PendingApproval -> Approved/Rejected
// -> Paid) - the accountant who files it cannot also approve it (see Permissions.Salary.Approve,
// deliberately withheld from the Accountant role).
public class SalaryPayment : ITenantScoped, IBranchScoped, IAuditableEntity, ISoftDeletable
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int BranchId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KWD";
    public DateOnly PeriodMonth { get; set; }
    public SalaryPaymentStatus Status { get; set; } = SalaryPaymentStatus.PendingApproval;
    public int? PaymentTypeId { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid? PaidByUserId { get; set; }
    public DateTime? PaidAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public Employee Employee { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public PaymentType? PaymentType { get; set; }
}
