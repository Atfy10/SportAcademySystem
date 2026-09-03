using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IUserBranchAccessRepository
    {
        Task<List<UserBranchAccess>> GetForUserAsync(Guid userId, CancellationToken ct = default);

        Task AddRangeAsync(IEnumerable<UserBranchAccess> access, CancellationToken ct = default);

        // Full replace: deletes every existing branch grant for the user not present in
        // `access`, upserts the rest - same replace-the-set semantics as
        // IUserPermissionOverrideRepository.ReplaceForUserAsync, used by the Users & Roles
        // "edit branches" action.
        Task ReplaceForUserAsync(Guid userId, Guid tenantId,
            IReadOnlyCollection<UserBranchAccess> access, CancellationToken ct = default);
    }
}
