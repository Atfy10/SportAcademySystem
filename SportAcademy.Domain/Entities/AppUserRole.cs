using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities;

public sealed class AppUserRole : IdentityUserRole<Guid>
{
    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;
}
