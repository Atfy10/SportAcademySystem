namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    public record SubscriptionStatsDto
    {
        public int Total { get; init; }
        public int Active { get; init; }
        public int Expired { get; init; }
        public int ExpiringSoon { get; init; }
        // Sold, but not started yet (start date in the future).
        public int Upcoming { get; init; }
        // Balance still owed and past its collect date.
        public int Overdue { get; init; }
    }
}
