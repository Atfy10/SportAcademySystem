using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories;

public class NotificationSettingsRepository : INotificationSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationEventType>> GetAllEventTypesAsync(CancellationToken ct = default)
        => await _context.Set<NotificationEventType>()
            .AsNoTracking()
            .OrderBy(e => e.DisplayName)
            .ToListAsync(ct);

    public async Task<Dictionary<(Guid EventTypeId, NotificationChannel Channel), bool>> GetTenantRulesAsync(
        Guid tenantId, CancellationToken ct = default)
        => (await _context.Set<TenantNotificationChannelRule>()
                .Where(r => r.TenantId == tenantId)
                .Select(r => new { r.EventTypeId, r.Channel, r.IsEnabled })
                .ToListAsync(ct))
            .ToDictionary(r => (r.EventTypeId, r.Channel), r => r.IsEnabled);

    public async Task<HashSet<NotificationChannel>> GetImplementedQueuedChannelsAsync(CancellationToken ct = default)
    {
        var implementedNames = await _context.Set<Feature>()
            .Where(f => (f.Name == "notifications-push" || f.Name == "notifications-whatsapp") && f.IsImplemented)
            .Select(f => f.Name)
            .ToListAsync(ct);

        var result = new HashSet<NotificationChannel>();
        if (implementedNames.Contains("notifications-push")) result.Add(NotificationChannel.Push);
        if (implementedNames.Contains("notifications-whatsapp")) result.Add(NotificationChannel.WhatsApp);
        return result;
    }

    public async Task UpsertTenantRulesAsync(
        Guid tenantId,
        IReadOnlyList<(Guid EventTypeId, NotificationChannel Channel, bool IsEnabled)> rules,
        string? updatedBy,
        CancellationToken ct = default)
    {
        if (rules.Count == 0) return;

        var eventTypeIds = rules.Select(r => r.EventTypeId).ToHashSet();
        var existing = await _context.Set<TenantNotificationChannelRule>()
            .Where(r => r.TenantId == tenantId && eventTypeIds.Contains(r.EventTypeId))
            .ToListAsync(ct);
        var existingByKey = existing.ToDictionary(r => (r.EventTypeId, r.Channel));

        var now = DateTime.UtcNow;
        foreach (var (eventTypeId, channel, isEnabled) in rules)
        {
            if (existingByKey.TryGetValue((eventTypeId, channel), out var row))
            {
                row.IsEnabled = isEnabled;
                row.UpdatedAt = now;
                row.UpdatedBy = updatedBy;
            }
            else
            {
                _context.Set<TenantNotificationChannelRule>().Add(new TenantNotificationChannelRule
                {
                    TenantId = tenantId,
                    EventTypeId = eventTypeId,
                    Channel = channel,
                    IsEnabled = isEnabled,
                    UpdatedAt = now,
                    UpdatedBy = updatedBy,
                });
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<Dictionary<NotificationChannel, bool>> GetUserPreferencesAsync(Guid userId, CancellationToken ct = default)
        => (await _context.Set<UserNotificationPreference>()
                .Where(p => p.UserId == userId)
                .Select(p => new { p.Channel, p.IsEnabled })
                .ToListAsync(ct))
            .ToDictionary(p => p.Channel, p => p.IsEnabled);

    public async Task UpsertUserPreferencesAsync(
        Guid userId,
        IReadOnlyList<(NotificationChannel Channel, bool IsEnabled)> preferences,
        CancellationToken ct = default)
    {
        if (preferences.Count == 0) return;

        var channels = preferences.Select(p => p.Channel).ToHashSet();
        var existing = await _context.Set<UserNotificationPreference>()
            .Where(p => p.UserId == userId && channels.Contains(p.Channel))
            .ToListAsync(ct);
        var existingByChannel = existing.ToDictionary(p => p.Channel);

        var now = DateTime.UtcNow;
        foreach (var (channel, isEnabled) in preferences)
        {
            if (existingByChannel.TryGetValue(channel, out var row))
            {
                row.IsEnabled = isEnabled;
                row.UpdatedAt = now;
            }
            else
            {
                _context.Set<UserNotificationPreference>().Add(new UserNotificationPreference
                {
                    UserId = userId,
                    Channel = channel,
                    IsEnabled = isEnabled,
                    UpdatedAt = now,
                });
            }
        }

        await _context.SaveChangesAsync(ct);
    }
}
