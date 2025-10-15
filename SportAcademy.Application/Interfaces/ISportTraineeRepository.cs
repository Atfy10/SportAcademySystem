using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface ISportTraineeRepository : IBaseRepository<SportTrainee,int>
	{
		Task<bool> CheckIfKeyExists(int sportId, int traineeId, CancellationToken cancellationToken);
		Task<SportTrainee?> GetByIdAsync(int sportId, int traineeId, CancellationToken cancellationToken);
	}
}
