namespace SportAcademy.Application.DTOs.TraineeDtos
{
    public class TraineeScheduleDto
    {
        public int TraineeGroupId { get; set; }

        public DayOfWeek Day { get; set; }

        public TimeOnly StartTime { get; set; }
    }
}
