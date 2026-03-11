using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public SkillLevel SkillLevel { get; init; }
    public Gender Gender { get; init; }
    public int MaximumCapacity { get; init; }
    public int DurationInMinutes { get; init; }
    public string SportName { get; init; } = null!;
    public string CoachName { get; init; } = null!;
    public string BranchName { get; init; } = null!;
    public int TraineesCount { get; init; }
    public List<GroupScheduleDto>? Schedules { get; init; }
}
