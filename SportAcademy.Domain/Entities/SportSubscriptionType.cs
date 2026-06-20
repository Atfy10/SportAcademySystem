namespace SportAcademy.Domain.Entities
{
    public class SportSubscriptionType
    {
        private SportSubscriptionType() { }

        private SportSubscriptionType(int sportId, int subscriptionTypeId)
        {
            SportId = sportId;
            SubscriptionTypeId = subscriptionTypeId;
        }

        public int SportId { get; private set; }
        public int SubscriptionTypeId { get; private set; }

        public virtual Sport Sport { get; private set; } = null!;
        public virtual SubscriptionType SubscriptionType { get; private set; } = null!;
        public virtual ICollection<SportPrice> SportPrices { get; private set; } = null!;

        public static SportSubscriptionType Create(int sportId, int subscriptionTypeId)
        {
            return new SportSubscriptionType(sportId, subscriptionTypeId);
        }
    }
}
