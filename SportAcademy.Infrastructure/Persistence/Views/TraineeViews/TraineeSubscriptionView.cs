namespace SportAcademy.Infrastructure.Persistence.Views.TraineeViews
{
    public class TraineeSubscriptionView
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public bool IsSubscribed { get; set; }

        public string GuardianName { get; set; } = null!;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int PaymentNumber { get; set; }

        public string SubscriptionTypeName { get; set; } = null!;

        public string SportName { get; set; } = null!;

        public string BranchName { get; set; } = null!;
    }
}
