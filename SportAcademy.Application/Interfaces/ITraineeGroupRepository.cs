using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeGroupRepository : IBaseRepository<TraineeGroup, int>
    {
        Task<List<ListTraineeGroupDto>> GetAllOfSpecificDayAsync(DateTime day, CancellationToken cancellationToken = default);
    }
}
