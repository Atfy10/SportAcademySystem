using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface ISessionOccurrenceRepository : IBaseRepository<SessionOccurrence, int>
{
    Task<PagedData<SessionOccurrenceDto>> GetAllPaginatedAsync(PageRequest page, CancellationToken cancellationToken = default);
    Task<PagedData<SessionOccurrenceDto>> GetByDateAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default);
    Task<PagedData<SessionOccurrenceDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default);
    Task<int?> GetTraineeGroupIdAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
