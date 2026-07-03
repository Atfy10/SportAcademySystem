using SportAcademy.Application.DTOs.AppUserDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Mappings;

public static class UserMapper
{
    public static MeResponse ToMeResponse(this AppUser user, List<string> roles)
        => new()
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            PhoneNumber = user.PhoneNumber,
            TenantId = user.TenantId,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };
}
