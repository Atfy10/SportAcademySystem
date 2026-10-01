using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.CoachDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ICoachRepository : IBaseRepository<Coach, int>
    {
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<double?> GetAverageRatingAsync(CancellationToken cancellationToken);
        Task<PagedData<CoachCardDto>> SearchAsync(
            string term,
            PageRequest pageReq,
            int? sportId,
            int? branchId,
            CancellationToken cancellationToken);
        Task<Coach?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>Looks up a Coach row for this employee even if it was soft-deleted - Coach
        /// shares its primary key with Employee 1:1, so re-creating a coach for an employee whose
        /// prior Coach record was deleted must recover that row instead of inserting a duplicate
        /// key.</summary>
        Task<Coach?> GetByEmployeeIdIncludingDeletedAsync(int employeeId, CancellationToken cancellationToken = default);
        Task<List<CoachDropdownItemDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default);

        /// <summary>What still references this coach and would make a hard delete lose data (or
        /// violate a foreign key). Looks across every branch and includes soft-deleted rows -
        /// the caller's own branch restrictions must not hide a blocker.</summary>
        Task<CoachRemovalBlockers> GetRemovalBlockersAsync(int employeeId, CancellationToken cancellationToken = default);

        /// <summary>Permanently removes the Coach row (and its per-branch access rows) - bypassing
        /// the soft-delete interceptor, unlike <see cref="IBaseRepository{T, TKey}"/> deletes. Call
        /// <see cref="GetRemovalBlockersAsync"/> first; the Employee row is untouched.</summary>
        Task HardDeleteAsync(int employeeId, CancellationToken cancellationToken = default);
    }

    public sealed record CoachRemovalBlockers(
        int GroupCount,
        IReadOnlyList<string> GroupNames,
        int TrainingHistoryCount)
    {
        public bool CanDelete => GroupCount == 0 && TrainingHistoryCount == 0;
    }
}
