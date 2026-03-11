namespace SportAcademy.Application.DTOs.GroupScheduleDtos;

public record GroupScheduleDto
{
    public int Id { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}
