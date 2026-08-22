namespace SportAcademy.Application.DTOs.Auth;

// Fresh, server-resolved roles/permissions for the current user - what the frontend should
// poll instead of trusting the (up to Jwt:ExpireMinutes stale) claims baked into its access
// token. See IPermissionResolver.
public record MyPermissionsDto(List<string> Roles, List<string> Permissions);
