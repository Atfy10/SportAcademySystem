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
}
