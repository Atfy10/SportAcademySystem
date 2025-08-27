namespace SportAcademy.Domain.Entities
{
    public class Session
    {
        public int Id { get; set; }
        public DayOfWeek Day { get; set; }
        public required string SkillLevel { get; set; }
        public int MaximumCapacity { get; set; } = 15;
        public TimeOnly StartTime { get; set; }
        public DateOnly Date { get; set; }
        public int DurationInMinutes { get; set; } = 55;
        public required string Gender { get; set; }
        public int BranchId { get; set; }
        public int CoachId { get; set; }

        // Navigation Property
        public virtual Branch Branch { get; set; } = null!;
        public virtual Coach Coach { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; } = [];
    }
}
