using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ISubscriptionDetailsRepository : IBaseRepository<SubscriptionDetails, int>
    {
        Task<SubscriptionDetails?> GetSubscriptionDetailsWithSubTypeAsync(int subscriptionId, CancellationToken cancellationToken = default);
        Task<int> GetTotalSessionsAllowed(int subDetailsId, CancellationToken cancellationToken = default);
        Task<List<SubscriptionDetails>?> GetSubscriptionDetailsForTraineeAsync(int traineeId, CancellationToken cancellationToken = default);
        Task<List<SubscriptionDetails>?> GetActiveSubscriptionDetailsForTraineeAsync(int traineeId, CancellationToken cancellationToken = default);
    }
}
