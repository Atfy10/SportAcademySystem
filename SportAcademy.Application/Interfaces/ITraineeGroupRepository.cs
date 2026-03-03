using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeGroupRepository : IBaseRepository<TraineeGroup, int>
    {
        Task<int> GetActiveTraineeGroupsCountAsync(CancellationToken cancellationToken = default);
        Task<PagedData<ListTraineeGroupDto>> GetAllOfSpecificDayAsync(PageRequest page, DateTime day, CancellationToken cancellationToken = default);

        Task<int> GetAllTraineeGroupsCountAsync(CancellationToken cancellationToken = default);
        Task<PagedData<TraineeGroupCardDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default);


    }
}
