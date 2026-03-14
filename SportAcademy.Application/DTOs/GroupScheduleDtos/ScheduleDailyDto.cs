namespace SportAcademy.Application.DTOs.GroupScheduleDtos
{
    public class ScheduleDailyDto
    {
        public int TraineeGroupId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeOnly StartTime { get; set; }
    }
}
