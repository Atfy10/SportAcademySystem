namespace SportAcademy.Application.DTOs.SubscriptionTypeDtos
{
    public class SubscriptionTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DaysPerMonth { get; set; }
        public int NumberOfMonths { get; set; }
        public bool IsActive { get; set; }
        public bool IsOffer { get; set; }
        public List<string> Sports { get; set; } = [];
        public List<int> SportIds { get; set; } = [];
    }
}