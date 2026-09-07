using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces;

// Read side of the tenant-status cache TenantStatusGuardMiddleware checks on every request.
// Split from ITenantStatusCacheInvalidator the same way IPermissionResolver is split from
// IPermissionCacheInvalidator: the middleware only ever reads, the handlers that change a
// tenant's status only ever invalidate, and neither needs to depend on the other's surface.
public interface ITenantStatusCache
{
    /// <summary>Null if no tenant with this id exists.</summary>
    Task<TenantStatus?> GetStatusAsync(Guid tenantId, CancellationToken ct = default);
}
