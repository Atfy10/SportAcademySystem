using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.DTOs.AppUserDtos;

// One row per catalog permission for the Owner-only override editor: what the user's role(s)
// grant by default, whether an explicit override exists, and the resulting effective value
// (Deny > Allow > role default).
public record UserPermissionStatusDto(string Permission, bool RoleDefault, PermissionEffect? Override, bool Effective);
