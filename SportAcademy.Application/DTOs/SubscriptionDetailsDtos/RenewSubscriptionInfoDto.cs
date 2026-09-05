using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    public record RenewSubscriptionInfoDto
    {
        public int Id { get; set; }
        public string TraineeName { get; set; } = null!;
        public string SportName { get; set; } = null!;
        public string BranchName { get; set; } = null!;
        public string SubscriptionTypeName { get; set; } = null!;
        public decimal Price { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int TraineeId { get; set; }
        public int SportId { get; set; }
        public int BranchId { get; set; }
        public int SubscriptionTypeId { get; set; }

        // Carried into the renewal so it reprices and re-dates on the same basis as the
        // subscription being renewed, instead of making staff re-pick both.
        public TraineeGroupType GroupType { get; set; }
        public List<DayOfWeek> TrainingDays { get; set; } = [];
    }
}
