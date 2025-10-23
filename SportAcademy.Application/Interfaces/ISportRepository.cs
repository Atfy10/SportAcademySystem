using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
	public interface ISportRepository : IBaseRepository<Sport, int>
	{
        Task<IEnumerable<Sport>> GetAvailableSportsForBranch(int branchId, CancellationToken cancellationToken);
		Task<bool> IsExistByNameAsync(string name, CancellationToken cancellationToken = default);

	}
}
