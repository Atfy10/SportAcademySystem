using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
	public interface ISportRepository : IBaseRepository<Sport, int>
	{
        Task<IEnumerable<Sport>> GetAvailableSportsForBranch(int branchId, CancellationToken cancellationToken);
		Task<bool> IsExistByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SportDropDownListDto>> SearchNameAsync(string term, CancellationToken cancellationToken = default);
        Task<PagedData<SportDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default);
    }
}
