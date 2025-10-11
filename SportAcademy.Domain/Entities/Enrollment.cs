namespace SportAcademy.Domain.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int SessionAllowed { get; set; }
        public int SessionRemaining { get; set; }
        public bool IsActive { get; set; }
        public int TraineeId { get; set; }
        public int TraineeGroupId { get; set; }

        // Navigation Properties
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual TraineeGroup TraineeGroup { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = [];
    }
}
