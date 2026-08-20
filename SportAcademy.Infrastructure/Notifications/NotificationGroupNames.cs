namespace SportAcademy.Infrastructure.Notifications;

/// <summary>
/// Builds tenant-scoped SignalR group names. Every real-time group in this system must be
/// built through here - a bare "General"/"Admins" string leaks across every tenant, since
/// SignalR groups and <see cref="Domain.Entities.NotificationGroupMember"/> rows carry no
/// tenant qualifier of their own.
/// </summary>
public static class NotificationGroupNames
{
    public const string General = "General";
    public const string Admins = "Admins";

    public static string ForTenant(Guid tenantId, string baseName) =>
        $"tenant:{tenantId:N}:{baseName.ToLowerInvariant()}";
}
