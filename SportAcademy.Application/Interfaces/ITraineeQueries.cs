using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeQueries
    {
        Task<IReadOnlyList<TraineeBasicDto>> GetTraineesAsync();

        Task<TraineeBasicDto?> GetTraineeByIdAsync(int traineeId);

        Task<IReadOnlyList<TraineeSubscriptionDto>> GetTraineeSubscriptionsAsync();

        Task<IReadOnlyList<TraineeSessionDto>> GetTraineeSessionsAsync();

        Task<IReadOnlyList<TraineeScheduleDto>> GetTraineeSchedulesAsync();

        Task<IReadOnlyList<TraineeAttendanceDto>> GetTraineeAttendanceAsync(int traineeId);
    }
}
