using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities.Tenants;

// Single source of truth for which TenantStatus transitions are legal. Previously
// ChangeTenantStatusCommandHandler and ArchiveTenantCommandHandler each hard-coded their own
// rules and disagreed: the status-change table forbade (Active, Archived) while the archive
// (DELETE) endpoint allowed it outright. Both handlers now call CanTransition so there is
// exactly one rule set, reachable by exactly one name.
public static class TenantStatusPolicy
{
    public static bool CanTransition(TenantStatus current, TenantStatus next)
    {
        if (current == next) return false;

        return (current, next) switch
        {
            (TenantStatus.PendingSetup, TenantStatus.Suspended) => true,
            (TenantStatus.Active, TenantStatus.Suspended) => true,
            (TenantStatus.Active, TenantStatus.Inactive) => true,
            (TenantStatus.Suspended, TenantStatus.Active) => true,
            (TenantStatus.Inactive, TenantStatus.Active) => true,
            (TenantStatus.Suspended, TenantStatus.Archived) => true,
            (TenantStatus.Inactive, TenantStatus.Archived) => true,
            // Archiving is not the end of the line: a mistakenly archived tenant (or one whose
            // customer comes back) can be brought back to Suspended - a deliberately inert
            // state - and re-activated from there as a second, separate decision. There is no
            // direct (Archived, Active) case: restoring straight to Active would skip that
            // decision.
            (TenantStatus.Archived, TenantStatus.Suspended) => true,
            _ => false,
        };
    }
}
