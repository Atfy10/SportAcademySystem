using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ISessionOccurrenceRepository : IBaseRepository<SessionOccurrence, int>
    {
        Task<PagedData<SessionOccurrenceCardDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default);
    }
}
