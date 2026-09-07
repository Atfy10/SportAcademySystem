namespace SportAcademy.Application.DTOs.PlatformDtos
{
    // Read model for both surfaces that list grants: the platform console (all tenants) and a
    // tenant's own Settings page (that tenant only, via GetTenantImpersonationHistoryQuery).
    // GrantedBy is a display name, not an id - a tenant Owner has no business resolving a
    // platform user's id, and the platform console already has the SuperAdmin's name from its
    // own session.
    public record TenantImpersonationGrantDto(
        Guid Id,
        Guid TenantId,
        string GrantedBy,
        string Reason,
        DateTime StartedAt,
        DateTime ExpiresAt,
        DateTime? EndedAt,
        string? EndedReason,
        bool IsActive);
}
