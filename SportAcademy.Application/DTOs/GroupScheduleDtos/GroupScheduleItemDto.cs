namespace SportAcademy.Application.DTOs.GroupScheduleDtos;

public record GroupScheduleItemDto
{
    public string DayOfWeek { get; init; } = null!;
    public string StartTime { get; init; } = null!;
}
