namespace SportAcademy.Domain.Entities
{
    public class SportSubscriptionType
    {
        public int SportId { get; set; }
        public int SubscriptionTypeId { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual SubscriptionType SubscriptionType { get; set; } = null!;
    }
}
