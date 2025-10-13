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
		Task<bool> CheckIfKeyExists(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken);
		Task<SportPrice> GetByKeyAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken);
		Task<List<SportPrice>> GetAllWithIncludesAsync(CancellationToken cancellationToken);
		Task<SportPrice> GetByKeyWithIncludesAsync(int branchId, int sportId, int subsTypeId, CancellationToken cancellationToken);
	}

}
