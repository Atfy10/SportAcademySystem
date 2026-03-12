namespace SportAcademy.Application.DTOs.GroupScheduleDtos;

public record GroupSchedulesTimesDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
}
