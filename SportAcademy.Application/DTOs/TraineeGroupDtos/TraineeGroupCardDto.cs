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
    public IReadOnlyList<GroupSchedulesTimesDto> Schedules { get; init; } = [];
}
