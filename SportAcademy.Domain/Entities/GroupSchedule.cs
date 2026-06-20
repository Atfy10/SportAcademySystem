namespace SportAcademy.Domain.Entities
{
    public class GroupSchedule
    {
        private GroupSchedule() { }

        private GroupSchedule(int traineeGroupId, DayOfWeek day, TimeOnly startTime)
        {
            TraineeGroupId = traineeGroupId;
            Day = day;
            StartTime = startTime;
        }

        public int Id { get; private set; }
        public int TraineeGroupId { get; private set; }
        public DayOfWeek Day { get; private set; }
        public TimeOnly StartTime { get; private set; }

        public virtual TraineeGroup TraineeGroup { get; private set; } = null!;
        public virtual ICollection<SessionOccurrence> SessionOccurrences { get; private set; } = [];

        public static GroupSchedule Create(int traineeGroupId, DayOfWeek day, TimeOnly startTime)
        {
            return new GroupSchedule(traineeGroupId, day, startTime);
        }
    }
}
