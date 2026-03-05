using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeRepository : IBaseRepository<Trainee, int>, IPersonRepository
    {
        Task<TraineeDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Trainee?> GetFullTrainee(int id, CancellationToken cancellationToken = default);
        Task<PagedData<TraineeOfSpecificDayDto>> GetAllTraineesOfSpecificDayAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default);
        Task<int> GetTraineesCountOfSpecificDayAsync(DateTime date, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<int> GetActiveTraineesCount(CancellationToken cancellationToken = default);
        Task<PagedData<TraineeCardDto>> SearchAsync(string term, PageRequest page, CancellationToken ct = default);
        Task<PagedData<TraineeCardDto>> SearchByIdAsync(int id, PageRequest page, CancellationToken ct = default);
    }
}
