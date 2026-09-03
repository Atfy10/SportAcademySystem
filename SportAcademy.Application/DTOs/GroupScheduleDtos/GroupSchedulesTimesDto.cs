namespace SportAcademy.Application.DTOs.GroupScheduleDtos;

public record GroupSchedulesTimesDto
{
    // string, not the DayOfWeek enum - see GroupScheduleDto.DayOfWeek's comment (camelCase
    // enum-over-the-wire vs the frontend's PascalCase enums.json translation keys).
    public string DayOfWeek { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
}
