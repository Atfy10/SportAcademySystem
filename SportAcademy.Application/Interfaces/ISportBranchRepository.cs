using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface ISportBranchRepository : IBaseRepository<SportBranch, int>
    {
        Task<bool> IsExistAsync(int sportId, int branchId, CancellationToken cancellationToken);
    }
}
