namespace SportAcademy.Application.DTOs.SubscriptionDetailsDtos
{
    public record SubscriptionStatsDto
    {
        public int Total { get; init; }
        public int Active { get; init; }
        public int Expired { get; init; }
        public int ExpiringSoon { get; init; }
    }
}
