namespace SportAcademy.Infrastructure.Persistence.Views.ScheduleViews;

public class ScheduleDailyView
{
    public int TraineeGroupId { get; set; }

    public DayOfWeek Day { get; set; }

    public TimeOnly StartTime { get; set; }
}
