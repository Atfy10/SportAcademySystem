using SportAcademy.Application.DTOs.CoachDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface ICoachQueries
    {
        Task<CoachSkillDto> GetCoachSkillsAsync();

        Task<IReadOnlyList<CoachScheduleDto>> GetCoachSchedulesAsync();
    }
}
