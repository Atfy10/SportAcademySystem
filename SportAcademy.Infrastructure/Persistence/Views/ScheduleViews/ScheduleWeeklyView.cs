using SportAcademy.Infrastructure.Persistence.Views.Interfaces;

namespace SportAcademy.Infrastructure.Persistence.Views.ScheduleViews;

public class ScheduleWeeklyView : IModelView
{
    public int TraineeGroupId { get; set; }

    public DayOfWeek Day { get; set; }

    public TimeOnly StartTime { get; set; }
}
