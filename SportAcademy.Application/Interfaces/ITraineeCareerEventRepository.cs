using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeCareerEventRepository : IBaseRepository<TraineeCareerEvent, int>
    {
        Task<List<TraineeCareerEvent>> GetSkillEventsForTraineeAsync(int traineeId, CancellationToken ct = default);
        Task<List<TraineeCareerEvent>> GetCoachEventsForTraineeAsync(int traineeId, CancellationToken ct = default);
    }
}
