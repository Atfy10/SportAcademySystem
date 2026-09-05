namespace SportAcademy.Application.DTOs.TraineeGroupDtos;

/// <summary>
/// One weekly training-day pattern in use for a sport/branch (e.g. Sun/Tue/Thu), plus how many
/// groups currently run it - the count lets the subscription form show which options actually
/// have capacity behind them rather than presenting every pattern as equally available.
/// </summary>
public record GroupDayPatternDto(
    List<DayOfWeek> Days,
    int GroupCount);
