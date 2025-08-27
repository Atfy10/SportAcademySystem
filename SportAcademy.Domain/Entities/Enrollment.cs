namespace SportAcademy.Domain.Entities
{
    public class Enrollment
    {
        public int Id { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int TraineeId { get; set; }
        public int SessionId { get; set; }

        // Navigation Properties
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual Session Session { get; set; } = null!;
        public virtual ICollection<Attendance> Attendances { get; set; } = [];
    }
}
