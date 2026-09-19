using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Implementations;

public class NotificationChannelDispatcher : INotificationChannelDispatcher
{
    private static readonly NotificationChannel[] QueuedChannels =
        [NotificationChannel.Email, NotificationChannel.Push, NotificationChannel.WhatsApp];

    private static readonly Dictionary<NotificationChannel, string> RequiredFeatureByChannel = new()
    {
        [NotificationChannel.Email] = "notifications-email",
        [NotificationChannel.Push] = "notifications-push",
        [NotificationChannel.WhatsApp] = "notifications-whatsapp",
    };

    private readonly ApplicationDbContext _context;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IContactResolver _contactResolver;
    private readonly ITenantRepository _tenantRepository;

    public NotificationChannelDispatcher(
        ApplicationDbContext context,
        ITenantIdProvider tenantIdProvider,
        IContactResolver contactResolver,
        ITenantRepository tenantRepository)
    {
        _context = context;
        _tenantIdProvider = tenantIdProvider;
        _contactResolver = contactResolver;
        _tenantRepository = tenantRepository;
    }

    public async Task DispatchAsync(
        int notificationId, IReadOnlyCollection<Guid> recipientUserIds, string eventTypeKey, CancellationToken ct = default)
    {
        if (recipientUserIds.Count == 0) return;

        var tenantId = _tenantIdProvider.TenantId;
        if (tenantId is null) return;

        var now = DateTime.UtcNow;

        // InApp: hardcoded always-enabled, never a matrix/preference lookup - the actual
        // persist+SignalR push already happened in NotificationService before this dispatcher
        // runs (see InAppChannelSender's remarks). This row is audit-only, recorded as Sent
        // immediately so the matrix/audit views stay consistent across every channel.
        var deliveries = recipientUserIds.Select(userId => new NotificationDelivery
        {
            TenantId = tenantId.Value,
            NotificationId = notificationId,
            RecipientUserId = userId,
            Channel = NotificationChannel.InApp,
            Status = NotificationDeliveryStatus.Sent,
            SentAt = now,
            CreatedAt = now,
            NextAttemptAt = now,
        }).ToList();

        var eventType = await _context.Set<NotificationEventType>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Key == eventTypeKey, ct);

        if (eventType is not null)
        {
            var rules = await _context.Set<TenantNotificationChannelRule>()
                .Where(r => r.TenantId == tenantId.Value && r.EventTypeId == eventType.Id)
                .ToDictionaryAsync(r => r.Channel, r => r.IsEnabled, ct);

            var matrixEnabledChannels = QueuedChannels
                .Where(c => rules.TryGetValue(c, out var enabled)
                    ? enabled
                    : c switch
                    {
                        // Every event that already reaches InApp/SignalR also reaches Push by
                        // default - Push rides along with every notification unless a tenant's
                        // own routing-matrix row (or the recipient's own preference, checked
                        // below) explicitly turns it off. Email keeps its narrower default (only
                        // the personally-actionable events) since it's a heavier, more visible
                        // channel; WhatsApp has no real provider behind it yet either way.
                        NotificationChannel.Email => NotificationEventTypes.DefaultEmailOnKeys.Contains(eventTypeKey),
                        NotificationChannel.Push => true,
                        _ => false,
                    })
                .ToList();

            // The routing matrix is a within-plan customization, not a way to unlock a channel
            // the tenant's plan doesn't actually grant - UpdateTenantNotificationChannelRulesCommand
            // is deliberately unguarded by IRequiresFeature (an Owner can toggle Push rows even on
            // a plan without notifications-email), so this is the one place that must actually
            // enforce the Feature/plan boundary before anything gets queued to send.
            var enabledChannels = new List<NotificationChannel>();
            foreach (var channel in matrixEnabledChannels)
            {
                if (!RequiredFeatureByChannel.TryGetValue(channel, out var featureKey)) continue;
                if (await _tenantRepository.IsFeatureEnabledAsync(tenantId.Value, featureKey, ct))
                    enabledChannels.Add(channel);
            }

            if (enabledChannels.Count > 0)
            {
                var prefLookup = (await _context.Set<UserNotificationPreference>()
                        .Where(p => recipientUserIds.Contains(p.UserId))
                        .ToListAsync(ct))
                    .ToDictionary(p => (p.UserId, p.Channel), p => p.IsEnabled);

                foreach (var userId in recipientUserIds)
                {
                    foreach (var channel in enabledChannels)
                    {
                        // A user can only narrow what the tenant's matrix already allows, never
                        // widen it - InApp is exempt from this lookup entirely (handled above).
                        if (prefLookup.TryGetValue((userId, channel), out var userEnabled) && !userEnabled)
                            continue;

                        var destination = channel switch
                        {
                            NotificationChannel.Email => await _contactResolver.ResolveEmailAsync(userId, ct),
                            NotificationChannel.WhatsApp => await _contactResolver.ResolvePhoneAsync(userId, ct),
                            _ => null,
                        };

                        deliveries.Add(new NotificationDelivery
                        {
                            TenantId = tenantId.Value,
                            NotificationId = notificationId,
                            RecipientUserId = userId,
                            Channel = channel,
                            Status = NotificationDeliveryStatus.Pending,
                            CreatedAt = now,
                            NextAttemptAt = now,
                            ResolvedDestination = destination,
                        });
                    }
                }
            }
        }

        _context.Set<NotificationDelivery>().AddRange(deliveries);
        await _context.SaveChangesAsync(ct);
    }
}
