namespace SportAcademy.Application.Interfaces;

// Write side of the tenant-status cache - see ITenantStatusCache. Every handler that changes
// Tenant.Status (ChangeTenantStatusCommandHandler, ArchiveTenantCommandHandler, and anything
// added later) must call Invalidate in the same request so the new status is visible to
// TenantStatusGuardMiddleware on the very next request, not just after the cache entry's TTL
// expires.
public interface ITenantStatusCacheInvalidator
{
    void Invalidate(Guid tenantId);
}
