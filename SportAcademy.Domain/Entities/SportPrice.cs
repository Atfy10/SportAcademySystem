namespace SportAcademy.Domain.Entities
{
    public class SportPrice
    {
        private SportPrice() { }

        private SportPrice(int sportId, int branchId, int subsTypeId, decimal price)
        {
            SportId = sportId;
            BranchId = branchId;
            SubsTypeId = subsTypeId;
            Price = price;
        }

        public int SportId { get; private set; }
        public int BranchId { get; private set; }
        public int SubsTypeId { get; private set; }
        public decimal Price { get; private set; }

        public virtual Branch Branch { get; private set; } = null!;
        public virtual SportSubscriptionType SportSubscriptionType { get; private set; } = null!;
        public virtual ICollection<SubscriptionDetails> SubscriptionsDetails { get; private set; } = null!;

        public static SportPrice Create(int sportId, int branchId, int subsTypeId, decimal price)
        {
            return new SportPrice(sportId, branchId, subsTypeId, price);
        }

        public void UpdatePrice(decimal newPrice)
        {
            Price = newPrice;
        }
    }
}
