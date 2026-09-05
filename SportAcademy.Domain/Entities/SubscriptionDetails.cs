using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SubscriptionDetails : ITenantScoped, IAuditableEntity, ISoftDeletable, IBranchScoped
    {
        public int Id { get; set; }
        public required DateOnly StartDate { get; set; }
        public required DateOnly EndDate { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public int TraineeId { get; set; }
        public int SubscriptionTypeId { get; set; }
        public int SportId { get; set; }
        public int BranchId { get; set; }

        /// <summary>
        /// Which kind of group this subscription was priced for. Part of the SportPrice key
        /// alongside Sport/Branch/SubscriptionType, so it's fixed at creation and never edited
        /// afterwards (same as SportId/BranchId). Only groups of this type may be enrolled into.
        /// </summary>
        public TraineeGroupType GroupType { get; set; }

        /// <summary>
        /// The weekly training-day pattern chosen at subscription time (e.g. Sun/Tue/Thu),
        /// picked from the patterns actually in use by groups for this sport+branch+type.
        /// EndDate is computed from it (see TrainingScheduleService.ComputeEndDate), and the
        /// group-assignment step narrows candidate groups to ones training on exactly these
        /// days - which is what keeps the billing period and the real schedule from drifting.
        /// Empty on subscriptions created before this was introduced.
        /// </summary>
        public List<DayOfWeek> TrainingDays { get; set; } = [];
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Property
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual SportPrice SportPrice { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;

        // Inverse of Finance.InvoiceLine.SubscriptionDetails - lets "is this subscription
        // paid" be expressed as a query-translatable predicate
        // (InvoiceLines.Any(l => l.Invoice.Status == InvoiceStatus.Paid)) instead of a live
        // join written out at every call site.
        public virtual ICollection<Finance.InvoiceLine> InvoiceLines { get; set; } = [];
    }
}
