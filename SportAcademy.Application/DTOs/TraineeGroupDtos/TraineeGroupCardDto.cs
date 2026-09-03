using SportAcademy.Application.DTOs.GroupScheduleDtos;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupCardDto
{
    public int Id { get; init; }
    public string Name { get; set; } = null!;
    public string SportName { get; init; } = null!;
    public string CoachName { get; init; } = null!;
    public string BranchName { get; init; } = null!;
    public int DurationInMinutes { get; init; }
    public int TraineesCount { get; init; }
    public int MaximumCapacity { get; init; }
    public string SkillLevel { get; init; } = null!;
    public bool IsActive { get; init; }
    public string? InactiveReason { get; init; }
    public IReadOnlyList<GroupSchedulesTimesDto> Schedules { get; init; } = [];
}
