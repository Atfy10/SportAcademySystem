namespace SportAcademy.Domain.Helpers;

/// <summary>
/// Names every notification audience in this system and builds their tenant-scoped SignalR
/// group names. Lives in Domain (not Infrastructure) specifically so Application-layer event
/// handlers can reference these constants directly instead of duplicating the raw strings.
/// </summary>
public static class NotificationGroupNames
{
    public const string General = "General";
    public const string Admins = "Admins";
    public const string Owners = "Owners";
    public const string Employees = "Employees";
    public const string Accountants = "Accountants";

    /// The three role-based audiences a notification can target. Membership for each is
    /// resolved live from role/employment data at send time (see
    /// NotificationService.ResolveRoleGroupMemberIdsAsync) - never from a connection-time
    /// cache - so a group notification always reaches every current member regardless of
    /// whether they've ever connected to the SignalR hub.
    public static readonly string[] AllRoleGroups = [Admins, Owners, Employees];

    public static string ForTenant(Guid tenantId, string baseName) =>
        $"tenant:{tenantId:N}:{baseName.ToLowerInvariant()}";
}
