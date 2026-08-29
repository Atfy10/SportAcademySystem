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
        Task<bool> AreIdsExistAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);

        /// <summary>All sports with Name/Description in the request language (falls back to English).</summary>
        Task<IReadOnlyList<SportDto>> GetAllTranslatedAsync(CancellationToken cancellationToken = default);

        /// <summary>Paginated sports with Name/Description in the request language.</summary>
        Task<PagedData<SportDto>> GetAllPaginatedTranslatedAsync(PageRequest page, CancellationToken cancellationToken = default);

        /// <summary>A single sport with Name/Description in the request language.</summary>
        Task<SportDto?> GetTranslatedByIdAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>Sports not yet linked to a branch, with Name/Description in the request language.</summary>
        Task<IReadOnlyList<SportDto>> GetAvailableSportsForBranchTranslatedAsync(int branchId, CancellationToken cancellationToken);
    }

}
