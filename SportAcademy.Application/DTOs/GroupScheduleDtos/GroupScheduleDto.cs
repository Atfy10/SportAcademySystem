namespace SportAcademy.Application.DTOs.GroupScheduleDtos;

public record GroupScheduleDto
{
    public int Id { get; init; }
    // string, not the DayOfWeek enum - the enum would round-trip through the API's global
    // camelCase JsonStringEnumConverter ("tuesday"), which doesn't match the frontend's
    // PascalCase enums.json translation keys ("Tuesday"). GroupScheduleItemDto (the list-view
    // equivalent) already sidesteps this the same way.
    public string DayOfWeek { get; init; } = null!;
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
}
