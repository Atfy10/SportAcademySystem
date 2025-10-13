using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface ISubscriptionTypeRepository : IBaseRepository<SubscriptionType, int>
	{
		Task<bool> CheckIfSubscriptionTypeExists(int subsTypeId, CancellationToken cancellationToken);
	}
}