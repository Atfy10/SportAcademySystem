using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities;

public class AppRole : IdentityRole<Guid>
{
    public ICollection<AppUserRole> UserRoles { get; set; } = [];
}
