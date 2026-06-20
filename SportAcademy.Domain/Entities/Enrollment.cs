using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Enrollment : IAuditableEntity, ISoftDeletable
    {
        private List<Attendance> _attendances = [];

        private Enrollment(
            DateTime enrollmentDate,
            DateTime expiryDate,
            int sessionAllowed,
            int traineeId,
            int traineeGroupId,
            int subscriptionDetailsId)
        {
            EnrollmentDate = enrollmentDate;
            ExpiryDate = expiryDate;
            SessionAllowed = sessionAllowed;
            SessionRemaining = sessionAllowed;
            IsActive = true;
            TraineeId = traineeId;
            TraineeGroupId = traineeGroupId;
            SubscriptionDetailsId = subscriptionDetailsId;
            _attendances = [];
        }

        private Enrollment() { }

        public int Id { get; private set; }
        public DateTime EnrollmentDate { get; private set; }
        public DateTime ExpiryDate { get; private set; }
        public int SessionAllowed { get; private set; }
        public int SessionRemaining { get; private set; }
        public bool IsActive { get; private set; }
        public int TraineeId { get; private set; }
        public int TraineeGroupId { get; private set; }
        public int SubscriptionDetailsId { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public virtual Trainee Trainee { get; set; } = null!;
        public virtual TraineeGroup TraineeGroup { get; set; } = null!;
        public IReadOnlyCollection<Attendance> Attendances => _attendances.AsReadOnly();
        public virtual SubscriptionDetails SubscriptionDetails { get; set; } = null!;

        public static Enrollment Create(
            DateTime enrollmentDate,
            DateTime expiryDate,
            int sessionAllowed,
            int traineeId,
            int traineeGroupId,
            int subscriptionDetailsId)
        {
            return new Enrollment(enrollmentDate, expiryDate, sessionAllowed,
                traineeId, traineeGroupId, subscriptionDetailsId);
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Suspend()
        {
            IsActive = false;
        }

        public void MarkDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void ExtendExpiry(DateTime? newExpiry)
        {
            if (newExpiry.HasValue)
                ExpiryDate = newExpiry.Value;
        }

        public void AdjustSessionRemaining(int? sessionRemaining)
        {
            if (sessionRemaining.HasValue)
                SessionRemaining = sessionRemaining.Value;
        }

        public string GetPaymentStatus()
        {
            if (ExpiryDate < DateTime.UtcNow) return "Overdue";
            if (SubscriptionDetails != null && SubscriptionDetails.Payment != null) return "Paid";
            return "Pending";
        }

        public string GetStatus()
        {
            if (ExpiryDate < DateTime.UtcNow) return "Expired";
            if (!IsActive) return "Suspended";
            return "Active";
        }

        public int GetSessionsCompleted()
            => _attendances.Count(a =>
                a.AttendanceStatus == AttendanceStatus.Present ||
                a.AttendanceStatus == AttendanceStatus.Late);
    }
}
