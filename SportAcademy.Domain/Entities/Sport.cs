using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Translations;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Sport : ITenantScoped
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public SportCategory Category { get; set; }
        public bool IsRequireHealthTest { get; set; } = true;

        // Mirrors Branch.IsActive exactly, added for plan-limit downgrade reconciliation (a
        // SuperAdmin-forced sport selection needs something to deactivate). Same scope as
        // Branch's own flag today: a toggle and a badge, not a read-path filter - existing
        // queries (GetAllTranslatedAsync, GetAvailableSportsForBranch, dropdown lists) are
        // deliberately left unfiltered here, matching BranchRepository.GetAllBranchsBase's
        // current behavior. Filtering pickers to active-only is a deliberate later decision,
        // not a side effect of adding this column.
        public bool IsActive { get; set; } = true;

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Properties
        public virtual ICollection<Coach> Coaches { get; set; } = [];
        public virtual ICollection<SportSubscriptionType> SubscriptionTypes { get; set; } = [];
        public virtual ICollection<SportBranch> Branches { get; set; } = [];
        public virtual ICollection<SportTrainee> Trainees { get; set; } = [];
        public virtual ICollection<SportTranslation> Translations { get; set; } = [];
	}
}
