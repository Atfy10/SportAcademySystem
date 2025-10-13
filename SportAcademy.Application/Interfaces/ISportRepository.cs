using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface ISportRepository : IBaseRepository<Sport, int>
	{
		Task<bool> CheckIfSportExists(int sportId, CancellationToken cancellationToken);
	}
}
