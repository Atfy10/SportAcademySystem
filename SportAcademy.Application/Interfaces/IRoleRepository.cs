using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task<bool> IsRoleExistAsync(string roleName, CancellationToken cancellationToken = default);
        Task<List<string>> GetRolesForUser(Guid userId, CancellationToken cancellationToken = default);
        Task<List<string>> GetAllRolesAsync(CancellationToken cancellationToken = default);
    }
}
