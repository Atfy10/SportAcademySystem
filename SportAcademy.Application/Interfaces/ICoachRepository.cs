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
            CancellationToken cancellationToken);
        Task<Coach?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
        /// <summary>Looks up a Coach row for this employee even if it was soft-deleted - Coach
        /// shares its primary key with Employee 1:1, so re-creating a coach for an employee whose
        /// prior Coach record was deleted must recover that row instead of inserting a duplicate
        /// key.</summary>
        Task<Coach?> GetByEmployeeIdIncludingDeletedAsync(int employeeId, CancellationToken cancellationToken = default);
        Task<List<CoachDropdownItemDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default);
    }
}
