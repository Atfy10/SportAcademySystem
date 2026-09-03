using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ICoachBranchAccessRepository
    {
        Task<List<CoachBranchAccess>> GetForCoachAsync(int coachId, CancellationToken ct = default);

        // Full replace: deletes every existing branch grant for the coach not present in
        // `access`, upserts the rest - same replace-the-set semantics as
        // IUserBranchAccessRepository.ReplaceForUserAsync, used by the coach profile's "Manage
        // branches" action.
        Task ReplaceForCoachAsync(int coachId, Guid tenantId,
            IReadOnlyCollection<CoachBranchAccess> access, CancellationToken ct = default);
    }
}
