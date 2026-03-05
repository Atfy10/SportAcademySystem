using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
	public interface IBranchRepository : IBaseRepository<Branch,int>
	{
		Task<List<BranchDropDownListDto>> GetAllBranchsBase(CancellationToken cancellationToken = default);
		Task<int> GetBranchesCountAsync(CancellationToken cancellationToken = default);
        Task<bool> IsEmailExistAsync(string email, CancellationToken cancellationToken = default);
		Task<bool> IsCoordinatesExistAsync(string coX, string coY, CancellationToken cancellationToken = default);
		Task<bool> IsPhoneNumberExistAsync(string phoneNumber, CancellationToken cancellationToken = default);
        Task<int> GetBranchTotalCapacityAsync(int branchId, CancellationToken cancellationToken = default);
    }
}
