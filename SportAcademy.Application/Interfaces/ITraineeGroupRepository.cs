using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeGroupRepository : IBaseRepository<TraineeGroup, int>
    {
        Task<PagedData<ListTraineeGroupDto>> GetAllOfSpecificDayAsync(PageRequest page, DateTime day, CancellationToken cancellationToken = default);
    }
}
