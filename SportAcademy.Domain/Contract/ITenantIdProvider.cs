namespace SportAcademy.Domain.Contract;

public interface ITenantIdProvider
{
    Guid? TenantId { get; }
    void SetTenantId(Guid? tenantId);

    /// <summary>
    /// Temporarily switches the ambient tenant to <paramref name="tenantId"/>, restoring the
    /// prior value when the returned handle is disposed. Use this instead of a bare
    /// <see cref="SetTenantId"/> call for any cross-tenant write (e.g. a SuperAdmin action
    /// against a different tenant's data) so the rest of the request - including anything
    /// added later, like audit logging - keeps running under the caller's real ambient tenant.
    /// </summary>
    IDisposable Impersonate(Guid tenantId);

    /// <summary>
    /// True while an explicit <see cref="AllowCrossTenantOperation"/> scope is active. Checked by
    /// <c>TenantSaveChangesInterceptor</c>: with no ambient tenant AND this false, saving any
    /// <see cref="ITenantScoped"/> entity is rejected outright rather than silently skipping the
    /// tenant check - "no tenant id" must mean "no successful write", not "unchecked write".
    /// </summary>
    bool AllowCrossTenantWrite { get; }

    /// <summary>
    /// Explicit, narrow opt-out of the no-ambient-tenant write guard above, for the handful of
    /// trusted background jobs that legitimately mutate rows across every tenant in one batch
    /// (e.g. EnrollmentLapseService, InvitationExpiryService) while running outside any HTTP
    /// request and therefore with no ambient tenant to set. Every row touched under this scope
    /// must already carry its own correct, non-empty TenantId (from an explicit per-row filter
    /// or join, never from IgnoreQueryFilters() alone) - the interceptor still rejects a save
    /// under this scope if it finds an ITenantScoped entity with TenantId == Guid.Empty.
    /// </summary>
    IDisposable AllowCrossTenantOperation();
}
