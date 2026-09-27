using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces;

public interface IPushSubscriptionRepository
{
    /// <summary>Upserts by Endpoint (unique) - re-subscribing the same browser/device profile
    /// updates the existing row (keys can rotate) rather than accumulating duplicates.</summary>
    Task AddOrUpdateAsync(PushSubscription subscription, CancellationToken ct = default);

    Task RemoveByEndpointAsync(Guid userId, string endpoint, CancellationToken ct = default);

    Task<List<PushSubscription>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Called by PushChannelSender when the push service reports a subscription gone
    /// (404/410) - the browser dropped it, so retrying it is pointless.</summary>
    Task RemoveAsync(int id, CancellationToken ct = default);
}
