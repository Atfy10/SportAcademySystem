using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Finance;

// A subscription that would use a discount code doesn't get created immediately (unlike a
// regular subscription) - it's queued here as PendingApproval and only actually created (via
// ISubscriptionCreationService) once an Owner/Admin/Accountant approves it. See
// Permissions.DiscountCode.Approve for who can review one; the requester (via
// Subscription.Manage) never can. Everything needed to later call
// ISubscriptionCreationService.CreateAsync is captured here at request time; the discount code
// and the SportPrice it applies to are both re-validated fresh at approval time, never trusted
// from the moment of request.
public class SubscriptionDiscountRequest : ITenantScoped, IBranchScoped, IAuditableEntity
{
    public int Id { get; set; }
    public int TraineeId { get; set; }
    public int SubscriptionTypeId { get; set; }
    public int SportId { get; set; }
    public int BranchId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int PaymentTypeId { get; set; }
    public required string DiscountCode { get; set; }   // raw code as typed, re-validated at approval time
    public SubscriptionDiscountRequestStatus Status { get; set; } = SubscriptionDiscountRequestStatus.PendingApproval;
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int? CreatedSubscriptionDetailsId { get; set; }  // set once Approved

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public virtual Trainee Trainee { get; set; } = null!;
    public virtual SubscriptionType SubscriptionType { get; set; } = null!;
    public virtual Sport Sport { get; set; } = null!;
    public virtual Branch Branch { get; set; } = null!;
    public virtual PaymentType PaymentType { get; set; } = null!;
    public virtual SubscriptionDetails? CreatedSubscriptionDetails { get; set; }
}
