using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces;

public interface INotificationSettingsRepository
{
    Task<List<NotificationEventType>> GetAllEventTypesAsync(CancellationToken ct = default);

    Task<Dictionary<(Guid EventTypeId, NotificationChannel Channel), bool>> GetTenantRulesAsync(
        Guid tenantId, CancellationToken ct = default);

    /// <summary>Which of the queued channels (Push, WhatsApp) have real code behind them yet -
    /// via their Feature.IsImplemented flag. InApp and Email are always considered implemented.</summary>
    Task<HashSet<NotificationChannel>> GetImplementedQueuedChannelsAsync(CancellationToken ct = default);

    Task UpsertTenantRulesAsync(
        Guid tenantId,
        IReadOnlyList<(Guid EventTypeId, NotificationChannel Channel, bool IsEnabled)> rules,
        string? updatedBy,
        CancellationToken ct = default);

    Task<Dictionary<NotificationChannel, bool>> GetUserPreferencesAsync(Guid userId, CancellationToken ct = default);

    Task UpsertUserPreferencesAsync(
        Guid userId,
        IReadOnlyList<(NotificationChannel Channel, bool IsEnabled)> preferences,
        CancellationToken ct = default);
}
