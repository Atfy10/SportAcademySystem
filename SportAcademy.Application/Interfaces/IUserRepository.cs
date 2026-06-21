using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IUserRepository : IBaseRepository<AppUser, string>
    {
        Task<IReadOnlyList<string?>> GetUserRoleAsync(AppUser user, CancellationToken ct = default);
        Task<IdentityResult> Register(AppUser user, string password);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<AppUser?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsUsernameExistAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default);
        Task<IdentityResult> AssignToRole(AppUser user, string role);
        Task<(IdentityResult Result, List<string> NotFoundRoles)> AssignToRolesAsync(AppUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
        Task<(IdentityResult Result, List<string> NotFoundRoles)> ReplaceRolesAsync(AppUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);
        Task<List<AppUser>> GetUnlinkedUsers(CancellationToken cancellationToken = default);
        Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
        Task<IdentityResult> AdminResetPasswordAsync(AppUser user, string newPassword);
    }
}
