using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IUserRepository : IBaseRepository<AppUser, Guid>
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

        // Cross-tenant lookups for the Platform/SuperAdmin console - the EF global query
        // filter scopes every normal query to the caller's own tenant, but a SuperAdmin
        // (whose own tenant is the platform-only "System" tenant) needs to see and act on
        // AppUsers that belong to OTHER tenants. IgnoreQueryFilters() is required here, same
        // pattern as TenantRepository's cross-tenant count methods.
        Task<AppUser?> GetByIdIgnoringTenantAsync(Guid id, CancellationToken ct = default);
        Task<(List<AppUser> Items, int TotalCount)> GetOwnersPagedAsync(int skip, int take, string? search, CancellationToken ct = default);
        Task<AppUser?> GetOwnerByIdAsync(Guid id, CancellationToken ct = default);

        // Split out of AdminResetPasswordAsync (which generates a token and immediately
        // consumes it in one call) so a "send a reset link" flow can generate the token now
        // and consume it later, from an unauthenticated request when the link is clicked.
        Task<string> GeneratePasswordResetTokenAsync(AppUser user);
        Task<IdentityResult> ConsumePasswordResetTokenAsync(AppUser user, string token, string newPassword);
    }
}
