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
    }
}
