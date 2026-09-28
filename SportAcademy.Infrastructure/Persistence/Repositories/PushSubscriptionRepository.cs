using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class PushSubscriptionRepository : IPushSubscriptionRepository
{
    private readonly ApplicationDbContext _context;

    public PushSubscriptionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddOrUpdateAsync(PushSubscription subscription, CancellationToken ct = default)
    {
        var existing = await _context.Set<PushSubscription>()
            .FirstOrDefaultAsync(s => s.Endpoint == subscription.Endpoint, ct);

        if (existing is null)
        {
            _context.Set<PushSubscription>().Add(subscription);
        }
        else
        {
            // Same browser/device re-subscribing (e.g. after clearing the old key pair) - keys
            // can rotate, the user might not be the same one anymore (shared device), so every
            // field is refreshed, not just touched.
            existing.UserId = subscription.UserId;
            existing.P256dh = subscription.P256dh;
            existing.Auth = subscription.Auth;
            existing.UserAgent = subscription.UserAgent;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task RemoveByEndpointAsync(Guid userId, string endpoint, CancellationToken ct = default)
    {
        var existing = await _context.Set<PushSubscription>()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint, ct);
        if (existing is null) return;

        _context.Set<PushSubscription>().Remove(existing);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<PushSubscription>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.Set<PushSubscription>()
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

    public async Task RemoveAsync(int id, CancellationToken ct = default)
    {
        var existing = await _context.Set<PushSubscription>().FirstOrDefaultAsync(s => s.Id == id, ct);
        if (existing is null) return;

        _context.Set<PushSubscription>().Remove(existing);
        await _context.SaveChangesAsync(ct);
    }
}
