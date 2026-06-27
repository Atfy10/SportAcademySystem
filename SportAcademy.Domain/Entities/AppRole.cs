using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities;

public class AppRole : IdentityRole<Guid>, ITenantScoped
{
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public ICollection<AppUserRole> UserRoles { get; set; } = [];
}
