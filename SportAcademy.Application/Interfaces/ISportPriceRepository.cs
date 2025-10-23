using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface ISportPriceRepository : IBaseRepository<SportPrice, int>
	{
		Task<bool> IsExistAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken = default);
		Task<SportPrice?> GetByKeyAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken = default);
		Task<List<SportPrice>> GetAllWithIncludesAsync(CancellationToken cancellationToken = default);
		Task<SportPrice?> GetByKeyWithIncludesAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken = default);
	}

}
