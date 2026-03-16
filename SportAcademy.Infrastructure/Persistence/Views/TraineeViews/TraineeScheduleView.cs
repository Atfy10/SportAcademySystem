using SportAcademy.Infrastructure.Persistence.Views.Interfaces;

namespace SportAcademy.Infrastructure.Persistence.Views.TraineeViews
{
    public class TraineeScheduleView : IModelView
    {
        public int TraineeGroupId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeOnly StartTime { get; set; }
    }
}
