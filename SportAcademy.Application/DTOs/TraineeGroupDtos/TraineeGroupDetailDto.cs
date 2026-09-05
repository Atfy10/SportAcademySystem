using SportAcademy.Application.DTOs.GroupScheduleDtos;

namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

public record TraineeGroupDetailDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    // string, not the SkillLevel/TraineeGroupGender enums - see GroupScheduleDto.DayOfWeek's
    // comment for why (camelCase-over-the-wire enum vs PascalCase-keyed enums.json).
    public string SkillLevel { get; init; } = null!;
    public string Gender { get; init; } = null!;
    /// <summary>"Public" or "Private" - a string for the same reason as the two above.</summary>
    public string Type { get; init; } = null!;
    public int MaximumCapacity { get; init; }
    public int DurationInMinutes { get; init; }
    public int SportId { get; init; }
    public string SportName { get; init; } = null!;
    public string CoachName { get; init; } = null!;
    public string BranchName { get; init; } = null!;
    public int TraineesCount { get; init; }
    public bool IsActive { get; init; }
    /// <summary>Staff-provided reason shown while the group is paused. Null while active.</summary>
    public string? InactiveReason { get; init; }
    public List<GroupScheduleDto>? Schedules { get; init; }
    public List<TraineeGroupMemberDto> Members { get; init; } = [];
}
