using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos
{
    public record ListTraineeGroupDto(
        int Id,
        string Name,
        string SportName,
        string CoachName,
        string BranchName,
        int DurationInMinutes,
        int TraineesCount,
        int MaximumCapacity,
        string SkillLevel,
        bool IsActive,
        string? InactiveReason,
        IReadOnlyList<GroupScheduleItemDto> Schedules,
        TraineeGroupType Type
    );
}
