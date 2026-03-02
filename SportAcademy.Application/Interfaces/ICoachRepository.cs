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
    }
}
