using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SubscriptionType
    {
        public int Id { get; set; }
        public SubType Name { get; set; }
        public int DaysPerMonth { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsOffer { get; set; } = false;

        // Navigation properties
        public virtual ICollection<SubscriptionDetails> SubscriptionDetails { get; set; } = [];
        public virtual ICollection<SportSubscriptionType> Sports { get; set; } = [];
        public virtual ICollection<SportPrice> SportPrices { get; set; } = [];
    }
}
