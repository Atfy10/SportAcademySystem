namespace SportAcademy.Application.Interfaces
{
    // Implemented by a command that assigns/creates something at a specific branch and must be
    // rejected if that branch has been deactivated - typically by the plan-limits downgrade
    // wizard (see PLAN_LIMITS_DESIGN.md D1/D5: a deselected branch is meant to become read-only,
    // not just invisible). See ActiveResourceGuardBehavior, which checks this the same way
    // BranchAccessValidationBehavior checks IBranchScopedRequest - a genuinely separate concern
    // (can the ACTING USER reach this branch at all, vs. is this branch even usable right now), so
    // a command commonly implements both. Never applied to a removal/deactivation command -
    // freeing something up must always be allowed, mirroring ToggleBranchStatusCommandHandler's
    // own "deactivating always frees a slot, never blocked" reasoning.
    public interface IRequiresActiveBranch
    {
        int BranchId { get; }
    }

    // Same idea for a partial-update command where BranchId is optional (only present when the
    // caller is actually reassigning the branch) - unset (null) means "leave it as-is" and is
    // never checked, mirroring IOptionallyBranchScopedRequest exactly.
    public interface IRequiresActiveOptionalBranch
    {
        int? BranchId { get; }
    }

    // Same idea for a command that assigns/creates something under a specific sport.
    public interface IRequiresActiveSport
    {
        int SportId { get; }
    }

    // Optional-SportId variant, for a partial-update command.
    public interface IRequiresActiveOptionalSport
    {
        int? SportId { get; }
    }

    // Same idea for a command that assigns a whole set of sports (e.g. a trainee's sport list, a
    // subscription type's eligible sports) - every id in the set must be active. Nullable
    // collection so a partial-update command whose SportIds is "not being changed this time"
    // (null) skips the check the same way an optional single id does.
    public interface IRequiresActiveSports
    {
        IEnumerable<int>? SportIds { get; }
    }
}
