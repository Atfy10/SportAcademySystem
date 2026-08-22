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
        public SubscriptionStatus Status { get; set; }
    }
}
