using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Common
{
    // Shared by UpdateCoachBranchesCommandHandler and UpdateUserBranchesCommandHandler - both are
    // full-replace-list commands (the whole branch-access set is resubmitted every time, not a
    // diff), so re-submitting a branch the target was already authorized for - even one that's
    // since been deactivated - must not break an otherwise-unrelated edit. Only a branch being
    // newly added needs to still be active (PLAN_LIMITS_DESIGN.md D1: "deactivated = no new
    // usage", not "instantly strip every existing reference").
    public static class NewlyAddedBranchGuard
    {
        /// <summary>Returns the id of the first newly-added, inactive-or-missing branch, or null
        /// if every newly-added branch is active (branches already in `existingBranchIds` are
        /// never checked, regardless of their current status).</summary>
        public static async Task<int?> FindInactiveNewlyAddedBranchAsync(
            IBranchRepository branchRepository,
            IEnumerable<int> requestedBranchIds,
            IEnumerable<int> existingBranchIds,
            CancellationToken ct)
        {
            var existing = existingBranchIds.ToHashSet();

            foreach (var branchId in requestedBranchIds.Distinct().Except(existing))
            {
                var branch = await branchRepository.GetByIdAsync(branchId, ct);
                if (branch is null || !branch.IsActive)
                    return branchId;
            }

            return null;
        }
    }
}
