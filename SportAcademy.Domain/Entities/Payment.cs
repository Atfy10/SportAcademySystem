using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Payment : IAuditableEntity
    {
        private Payment(
            string paymentNumber,
            PaymentMethod method,
            int branchId)
        {
            PaymentNumber = paymentNumber;
            Method = method;
            BranchId = branchId;
            PaidDate = DateTime.UtcNow;
        }

        private Payment() { }

        public string PaymentNumber { get; private set; } = null!;
        public PaymentMethod Method { get; private set; }
        public DateTime PaidDate { get; private set; }
        public int BranchId { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual Branch Branch { get; set; } = null!;
        public virtual SubscriptionDetails SubscriptionDetails { get; set; } = null!;

        public static Payment Create(
            string paymentNumber,
            PaymentMethod method,
            int branchId)
        {
            return new Payment(paymentNumber, method, branchId);
        }
    }
}
