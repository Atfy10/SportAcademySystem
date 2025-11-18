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
        Task<IdentityResult> Register(AppUser user, string password);
        Task<bool> CheckPasswordAsync(AppUser user, string password);
        Task<AppUser?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken ct = default);
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsUsernameExistAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default);
        Task<IdentityResult> AssignToRole(AppUser user, string role);
    }
}
