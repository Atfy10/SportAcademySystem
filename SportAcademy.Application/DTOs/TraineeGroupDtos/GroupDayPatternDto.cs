namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

/// <summary>
/// One weekly training-day pattern in use for a sport/branch (e.g. Sun/Tue/Thu), plus how many
/// groups currently run it - the count lets the subscription form show which options actually
/// have capacity behind them rather than presenting every pattern as equally available.
/// </summary>
/// <remarks>
/// Days are day-of-week names ("Sunday", "Tuesday"), matching every other day-of-week field
/// this API exposes (see GroupScheduleItemDto.DayOfWeek) and the client's enums.dayOfWeek
/// translation keys. Not raw DayOfWeek values: the global JsonStringEnumConverter is configured
/// with camelCase naming and allowIntegerValues:false, so those would serialize as "sunday" -
/// inconsistent with the rest of the API, and integers wouldn't round-trip back at all.
/// </remarks>
public record GroupDayPatternDto(
    List<string> Days,
    int GroupCount);
