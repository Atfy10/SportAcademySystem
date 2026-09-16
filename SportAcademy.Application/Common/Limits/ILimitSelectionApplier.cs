namespace SportAcademy.Application.Common.Limits;

// Backs SubmitLimitSelectionCommandHandler - the one operation that needs to read/write across
// Branch, Sport, AppUser, TraineeGroup, UserBranchAccess and CoachBranchAccess together. Kept as
// its own narrow service (Infrastructure has direct DbContext access) rather than growing
// IBranchRepository/ISportRepository/IUserRepository/ITenantRepository with methods only this
// one command needs.
public interface ILimitSelectionApplier
{
    /// <summary>
    /// Applies the wizard's explicit selection (deactivates every branch/sport/user of this
    /// tenant NOT in the given keep-lists) and then independently enforces the D5 cascade
    /// server-side, regardless of what the client submitted: a branch-restricted (Employee-role)
    /// user or a coach whose branch access is now entirely within deactivated branches is banned
    /// too, and every TraineeGroup at a deactivated branch is paused. Stages changes only - the
    /// caller's own SaveChangesAsync persists them.
    /// </summary>
    Task<LimitSelectionApplyResult> ApplyAsync(
        Guid tenantId, IReadOnlyCollection<int> keepBranchIds, IReadOnlyCollection<int> keepSportIds,
        IReadOnlyCollection<Guid> keepUserIds, CancellationToken ct = default);
}

public record LimitSelectionApplyResult(
    int DeactivatedBranches, int DeactivatedSports, int BannedUsers, int CascadeBannedUsers, int DeactivatedGroups);
