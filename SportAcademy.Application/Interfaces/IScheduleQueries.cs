using SportAcademy.Application.DTOs.GroupScheduleDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface IScheduleQueries
    {
        Task<IReadOnlyList<ScheduleDailyDto>> GetDailyScheduleAsync();

        Task<IReadOnlyList<ScheduleWeeklyDto>> GetWeeklyScheduleAsync();
    }
}
