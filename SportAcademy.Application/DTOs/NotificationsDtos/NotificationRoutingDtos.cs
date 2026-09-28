namespace SportAcademy.Application.DTOs.NotificationsDtos;

/// <summary>One (event type, channel) cell of the tenant's routing matrix. IsDefault is true
/// when no TenantNotificationChannelRule row backs this cell yet - IsEnabled then reflects
/// NotificationEventTypes.DefaultEmailOnKeys (or the InApp/hardcoded-on and Push/WhatsApp/
/// hardcoded-off cases) rather than an explicit tenant choice.</summary>
public record NotificationMatrixCellDto(
    string EventTypeKey,
    string EventTypeDisplayName,
    string? EventTypeDescription,
    string Channel,
    bool IsEnabled,
    bool IsDefault,
    bool IsChannelImplemented);

/// <summary>One rule update in a bulk save - Channel "InApp" is rejected by the handler, it's
/// never user-configurable.</summary>
public record NotificationRuleUpdateDto(string EventTypeKey, string Channel, bool IsEnabled);

public record NotificationPreferenceDto(string Channel, bool IsEnabled, bool IsChannelImplemented);

public record NotificationPreferenceUpdateDto(string Channel, bool IsEnabled);
