using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.ExcuseRequestDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface IExcuseRequestRepository : IBaseRepository<ExcuseRequest, int>
{
    Task<bool> ExistsPendingAsync(int sessionOccurrenceId, int traineeId, CancellationToken cancellationToken = default);
    Task<PagedData<ExcuseRequestDto>> GetPendingAsync(PageRequest page, CancellationToken cancellationToken = default);
    Task<int> CountPendingAsync(CancellationToken cancellationToken = default);
}
