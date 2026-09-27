namespace SportAcademy.Application.Interfaces;

/// <summary>
/// Resolves a user's contact destination for a channel. Precedence:
/// Employee's own contact info, else Trainee's, else the Identity login email (last resort) -
/// most users are exactly one of Employee/Trainee, but a login always has an Identity email.
/// </summary>
public interface IContactResolver
{
    Task<string?> ResolveEmailAsync(Guid userId, CancellationToken ct = default);
    Task<string?> ResolvePhoneAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Same precedence as the single-user methods, batched into one query for a whole recipient
    /// list - for callers (NotificationChannelDispatcher) that already have every recipient in
    /// hand and would otherwise pay one round trip per (recipient, channel) pair. A missing key
    /// means no contact was resolvable for that user (never a distinct "not found" case).
    /// </summary>
    Task<Dictionary<Guid, (string? Email, string? Phone)>> ResolveContactsAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken ct = default);
}
