namespace SportAcademy.Domain.Entities
{
    public class SportPrice
    {
        public int SportId { get; set; }
        public int BranchId { get; set; }
        public int SubsTypeId { get; set; }
        public decimal Price { get; set; }

        // Navigation Property
        public virtual Sport Sport { get; set; } = null!;
        public virtual Branch Branch { get; set; } = null!;
        public virtual SubscriptionType SubscriptionType { get; set; } = null!;
    }
}
