namespace SportAcademy.Application.Interfaces
{
    // Computes the permission set a user actually has right now: role defaults, layered with
    // that user's own Allow/Deny overrides, with Deny always winning. This is deliberately a
    // server-side, per-request resolution rather than something read off the JWT - a Deny that
    // only takes effect on the next token refresh (up to Jwt:ExpireMinutes later) would not be
    // a real Deny. See PermissionResolver for the caching/invalidation strategy.
    public interface IPermissionResolver
    {
        Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default);

        Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default);
    }

    // Call after any write that can change a user's effective permissions (override edit, role
    // assignment/removal, invitation acceptance, activation toggle) so the next authorization
    // check re-resolves instead of serving a stale cached set.
    public interface IPermissionCacheInvalidator
    {
        void Invalidate(Guid userId);
    }
}
