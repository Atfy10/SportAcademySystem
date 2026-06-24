using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleRepository(
            RoleManager<AppRole> roleManager,
            UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<List<string>> GetRolesForUser(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new IdNotFoundException(nameof(AppUser), userId);

            var roles = await _userManager.GetRolesAsync(user);

            return roles.ToList();
        }

        public async Task<bool> IsRoleExistAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
            foreach (var role in roles)
            {
                if (role.Name == roleName)
                    return true;
            }
            return false;
        }

        public async Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default)
            => await _roleManager.Roles.Select(r => r.Name!).ToListAsync(cancellationToken);
    }
}
