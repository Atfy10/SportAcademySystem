using SportAcademy.Application.DTOs.TraineeGroupDtos;

namespace SportAcademy.Application.Interfaces
{
    public interface IGroupQueries
    {
        Task<IReadOnlyList<GroupsViewDto>> GetGroupsAsync();

        Task<GroupsViewDto?> GetGroupByIdAsync(int groupId);
    }
}
