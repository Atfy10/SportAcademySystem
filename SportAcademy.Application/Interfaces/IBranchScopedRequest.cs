namespace SportAcademy.Application.Interfaces
{
    // Implemented by commands that create/target a specific branch (BranchId is always
    // supplied, e.g. CreateEmployeeCommand). See BranchAccessValidationBehavior: a
    // branch-restricted "Employee" caller (IBranchAccessProvider.IsRestricted) is rejected with
    // 403 up front if BranchId isn't in their allowed set - without this, the entity-level EF
    // query filter only protects *reads*; nothing stopped a restricted caller from writing a
    // brand-new row into a branch they can't see.
    public interface IBranchScopedRequest
    {
        int BranchId { get; }
    }

    // Same idea for a partial-update command where BranchId is optional (only present when the
    // caller is actually reassigning the branch, e.g. UpdateEmployeeCommand) - unset (null)
    // means "leave it as-is" and is never checked.
    public interface IOptionallyBranchScopedRequest
    {
        int? BranchId { get; }
    }
}
