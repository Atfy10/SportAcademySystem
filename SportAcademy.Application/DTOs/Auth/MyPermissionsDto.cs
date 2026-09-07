using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.Auth;

// Fresh, server-resolved roles/permissions for the current user - what the frontend should
// poll instead of trusting the (up to Jwt:ExpireMinutes stale) claims baked into its access
// token. See IPermissionResolver.
//
// IsActive and TenantStatus ride along on the same poll (F-02): a ban or a tenant
// suspend/archive/deactivate already kills the session server-side (TenantStatusGuardMiddleware,
// JwtTokenService's refresh check), but without these fields the frontend would only find out
// the next time some other request happened to 403 - these let AuthContext's existing polling
// loop notice and log the session out within one poll (60s) instead.
public record MyPermissionsDto(
    List<string> Roles,
    List<string> Permissions,
    bool IsActive,
    TenantStatus TenantStatus);
