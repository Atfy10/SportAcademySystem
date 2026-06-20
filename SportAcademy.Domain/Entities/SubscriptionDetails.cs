using SportAcademy.Domain.Contract;

namespace SportAcademy.Domain.Entities
{
    public class SubscriptionDetails : IAuditableEntity, ISoftDeletable
    {
        private SubscriptionDetails(
            DateOnly startDate,
            DateOnly endDate,
            string paymentNumber,
            int traineeId,
            int subscriptionTypeId,
            int sportId,
            int branchId)
        {
            StartDate = startDate;
            EndDate = endDate;
            PaymentNumber = paymentNumber;
            TraineeId = traineeId;
            SubscriptionTypeId = subscriptionTypeId;
            SportId = sportId;
            BranchId = branchId;
            IsActive = startDate <= DateOnly.FromDateTime(DateTime.Now)
                && endDate >= DateOnly.FromDateTime(DateTime.Now);
        }

        private SubscriptionDetails() { }

        public int Id { get; private set; }
        public DateOnly StartDate { get; private set; }
        public DateOnly EndDate { get; private set; }
        public bool IsActive { get; private set; }
        public string PaymentNumber { get; private set; } = null!;
        public int TraineeId { get; private set; }
        public int SubscriptionTypeId { get; private set; }
        public int SportId { get; private set; }
        public int BranchId { get; private set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual Payment Payment { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual SportPrice SportPrice { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;

        public int AllowedSessionsPerMonth
            => SportPrice?.SportSubscriptionType?.SubscriptionType?.DaysPerMonth ?? 0;

        public static SubscriptionDetails Create(
            DateOnly startDate,
            DateOnly endDate,
            string paymentNumber,
            int traineeId,
            int subscriptionTypeId,
            int sportId,
            int branchId)
        {
            if (startDate >= endDate)
                throw new ArgumentException("Start date must be before end date.");

            return new SubscriptionDetails(startDate, endDate, paymentNumber,
                traineeId, subscriptionTypeId, sportId, branchId);
        }

        public static bool HasDateConflictWith(
            SubscriptionDetails sub,
            List<SubscriptionDetails>? activeSubscriptions)
        {
            if (activeSubscriptions == null || activeSubscriptions.Count == 0)
                return false;

            foreach (var activeSub in activeSubscriptions)
            {
                if ((sub.StartDate <= activeSub.EndDate
                    || sub.EndDate >= activeSub.StartDate)
                    && sub.SportId == activeSub.SportId)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateDates(DateOnly? startDate, DateOnly? endDate)
        {
            if (startDate.HasValue)
                StartDate = startDate.Value;
            if (endDate.HasValue)
                EndDate = endDate.Value;
            IsActive = StartDate <= DateOnly.FromDateTime(DateTime.Now)
                && EndDate >= DateOnly.FromDateTime(DateTime.Now);
        }

        public void UpdatePayment(string? paymentNumber)
        {
            if (!string.IsNullOrWhiteSpace(paymentNumber))
                PaymentNumber = paymentNumber;
        }

        public void MarkDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }
    }
}
