using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Extensions;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class UserRepository : BaseRepository<AppUser, string>, IUserRepository
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager) : base(context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<IReadOnlyList<string?>> GetUserRoleAsync(AppUser user, CancellationToken ct = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<IdentityResult> Register(AppUser user, string password)
        {
            var registerResult = await _userManager.CreateAsync(user, password);
            if (!registerResult.Succeeded)
                return registerResult;

            var assignToRoleResult = await _userManager.AddToRoleAsync(user, "User");
            return assignToRoleResult;
        }

        public async Task<AppUser?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default)
        {
            var user = await _userManager.FindByNameAsync(usernameOrEmail);

            if (user != null)
                return user;

            user = await _userManager.FindByEmailAsync(usernameOrEmail);
            return user;
        }

        public async Task<bool> CheckPasswordAsync(AppUser user, string password)
            => await _userManager.CheckPasswordAsync(user, password);

        public override async Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            await _userManager.CreateAsync(user, $"{user.UserName}1234");
        }

        public async Task<IdentityResult> AssignToRole(AppUser user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            return result;
        }

        public async Task<(IdentityResult Result, List<string> NotFoundRoles)> AssignToRolesAsync(AppUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            var notFoundRoles = new List<string>();

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    notFoundRoles.Add(role);
            }

            if (notFoundRoles.Count > 0)
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "RolesNotFound",
                    Description = $"The following roles do not exist: {string.Join(", ", notFoundRoles)}"
                }), notFoundRoles);

            var result = await _userManager.AddToRolesAsync(user, roles);
            return (result, notFoundRoles);
        }

        public async Task<(IdentityResult Result, List<string> NotFoundRoles)> ReplaceRolesAsync(AppUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    return (removeResult, []);
            }

            var notFoundRoles = new List<string>();

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    notFoundRoles.Add(role);
            }

            if (notFoundRoles.Count > 0)
                return (IdentityResult.Failed(new IdentityError
                {
                    Code = "RolesNotFound",
                    Description = $"The following roles do not exist: {string.Join(", ", notFoundRoles)}"
                }), notFoundRoles);

            var result = await _userManager.AddToRolesAsync(user, roles);
            return (result, notFoundRoles);
        }

        public override async Task UpdateAsync(AppUser entity, CancellationToken cancellationToken = default)
        {
            await _userManager.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(AppUser entity, CancellationToken cancellationToken = default)
        {
            await _userManager.DeleteAsync(entity);
        }

        public override async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await _userManager.DeleteAsync(await GetByIdAsync(id, cancellationToken)
                ?? throw new IdNotFoundException(EntityTypes.User.DisplayName(), id.ToString()));
        }

        public override async Task<AppUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
            => await _userManager.FindByIdAsync(id);

        public override async Task<List<AppUser>> GetAllAsync(CancellationToken cancellationToken = default)
            => await _userManager.Users.ToListAsync(cancellationToken);

        public async Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => await _userManager.FindByEmailAsync(email);

        public async Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
            => await _userManager.FindByNameAsync(username);

        public async Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default)
            => await _context.AppUsers.AnyAsync(u => u.Email == email, cancellationToken);

        public async Task<bool> IsUsernameExistAsync(string username, CancellationToken cancellationToken = default)
            => await _context.AppUsers.AnyAsync(u => u.UserName == username, cancellationToken);

        public async Task<List<AppUser>> GetUnlinkedUsers(CancellationToken cancellationToken = default)
            => await _context.AppUsers
                .Where(u => u.Employee == null && u.Trainee == null)
                .ToListAsync(cancellationToken);

        public async Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
            => await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        public async Task<IdentityResult> AdminResetPasswordAsync(AppUser user, string newPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}
