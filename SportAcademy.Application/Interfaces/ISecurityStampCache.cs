namespace SportAcademy.Application.Interfaces;

/// What the per-request access-token check needs to know about a user: the SecurityStamp their
/// token must still match, and whether they are allowed in at all.
public sealed record UserSessionState(string? SecurityStamp, bool IsBanned, bool IsDeleted);

// Read side of the cache the JWT bearer OnTokenValidated check reads on every authenticated
// request (see AccessTokenSessionValidator). Split from ISecurityStampCacheInvalidator the same way
// ITenantStatusCache is split from ITenantStatusCacheInvalidator.
public interface ISecurityStampCache
{
    /// <summary>Null if no user with this id exists.</summary>
    Task<UserSessionState?> GetAsync(Guid userId, CancellationToken ct = default);
}

// Write side - see ISecurityStampCache. ISessionRevocationService calls this right after
// rotating the stamp so the old one stops being accepted on the very next request.
public interface ISecurityStampCacheInvalidator
{
    void Invalidate(Guid userId);
}
