using SportAcademy.Application.DTOs.PaymentDtos;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    public record SubscriptionDetailsDto
    {
        public TraineeSubDetailsDto Trainee { get; set; } = null!;
        public string SportName { get; set; } = null!;
        public string BranchName { get; set; } = null!;
        public string SubscriptionTypeName { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string EmployeeName { get; set; } = null!;
        public PaymentSubDetailsDto Payment { get; set; } = null!;
    }
}
