using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    public record SubscriptionDetailsDto
    {
        public int Id { get; set; }
        public TraineeSubDetailsDto Trainee { get; set; } = null!;
        public string SportName { get; set; } = null!;
        public string BranchName { get; set; } = null!;
        public string SubscriptionTypeName { get; set; } = null!;
        public decimal Price { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public string EmployeeName { get; set; } = null!;
        // Null until at least one payment has been recorded against this subscription's
        // invoice - previously a Payment row was fabricated synchronously at subscription
        // creation regardless of whether money had actually changed hands, which this
        // corrects. Reflects the most recently received payment when several exist.
        public PaymentSubDetailsDto? Payment { get; set; }
        // Effective status: Active / Upcoming (starts later) / Suspended / Expired - see
        // SubscriptionBilling.EffectiveStatus. Never the raw stored flag.
        public SubscriptionStatus Status { get; set; }

        // The bill behind this subscription. Price above is the invoice total (after any discount);
        // these say how much of it is settled and, if not, by when it has to be.
        public string? InvoiceNumber { get; set; }
        public int? InvoiceId { get; set; }
        // The branch the bill belongs to - a balance must be collected at that branch.
        public int? InvoiceBranchId { get; set; }
        public string? Currency { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }
        public DateOnly? BalanceDueDate { get; set; }
        public SubscriptionPaymentState? PaymentState { get; set; }
    }
}
