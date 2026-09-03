using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities;

// One row per (CoachId, BranchId) a coach is authorized to train at - mirrors UserBranchAccess's
// shape exactly, but for "which branches can this coach teach groups at" instead of "which
// branches can this staff member see data for". A coach's Employee.BranchId is only their
// *employment* branch (HR/payroll) and is deliberately not used for this - see the
// branchAutoFilterExclusions note in ApplicationDbContext.OnModelCreating for why conflating the
// two broke trainee-group/session/enrollment visibility. New coaches are backfilled with a single
// row for their employment branch (migration AddCoachBranchAccess) so existing behavior doesn't
// change until an admin explicitly adds more branches via the coach's "Manage branches" action.
public class CoachBranchAccess : ITenantScoped, IAuditableEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int CoachId { get; set; }
    public int BranchId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public Coach Coach { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
