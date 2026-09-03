namespace SportAcademy.Domain.Contract;

/// Per-request (AsyncLocal-backed, same pattern as ITenantIdProvider) branch-access state for
/// the current authenticated user. Resolved once by middleware in Program.cs, after
/// ITenantIdProvider is populated, and read by ApplicationDbContext's branch-scoped query
/// filters. Only "Employee"-role users are ever restricted - every other role sees every
/// branch (IsRestricted stays false), matching Employee.BranchId being the tenant's normal
/// "unrestricted staff" role.
public interface IBranchAccessProvider
{
    bool IsRestricted { get; }
    IReadOnlyList<int> AllowedBranchIds { get; }
    void SetBranchAccess(bool isRestricted, IReadOnlyList<int> allowedBranchIds);
}
