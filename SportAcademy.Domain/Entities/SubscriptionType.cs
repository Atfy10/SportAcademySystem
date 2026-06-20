using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class SubscriptionType
    {
        private SubscriptionType() { }

        private SubscriptionType(string name, int daysPerMonth, bool isActive, bool isOffer)
        {
            Name = name;
            DaysPerMonth = daysPerMonth;
            IsActive = isActive;
            IsOffer = isOffer;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public int DaysPerMonth { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsOffer { get; private set; } = false;

        public virtual ICollection<SportSubscriptionType> Sports { get; private set; } = [];

        public static SubscriptionType Create(string name, int daysPerMonth, bool isActive, bool isOffer)
        {
            return new SubscriptionType(name, daysPerMonth, isActive, isOffer);
        }
    }
}
