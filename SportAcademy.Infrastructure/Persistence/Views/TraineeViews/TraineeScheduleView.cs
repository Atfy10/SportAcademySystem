namespace SportAcademy.Infrastructure.Persistence.Views.TraineeViews
{
    public class TraineeScheduleView
    {
        public int TraineeGroupId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeOnly StartTime { get; set; }
    }
}
