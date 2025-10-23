using SportAcademy.Domain.Entities;
using SportAcademy.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
	public interface IBranchRepository : IBaseRepository<Branch,int>
	{
		Task<bool> IsEmailExistAsync(string? email, CancellationToken cancellationToken);
		Task<bool> IsCoordinatesExistAsync(Coordinate coordinate, CancellationToken cancellationToken);
		Task<bool> IsPhoneNumberExistAsync(string phoneNumber, CancellationToken cancellationToken);
	}
}
