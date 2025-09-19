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
        Task<AppUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<AppUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> IsUsernameExistAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default);
    }
}
