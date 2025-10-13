using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface IBranchRepository : IBaseRepository<Branch,int>
	{
		Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken);
		Task<bool> IsCoordinatesExistAsync(string coX, string coY, CancellationToken cancellationToken);
		Task<bool> IsPhoneNumberExistAsync(string phoneNumber, CancellationToken cancellationToken);
		Task<bool> CheckIfBranchExists(int branchId, CancellationToken cancellationToken);
	}
}
