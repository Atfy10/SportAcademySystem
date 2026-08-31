using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.FamilyDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface IFamilyRepository : IBaseRepository<Family, int>
    {
        Task<IReadOnlyList<TFamilyDto>> SearchFamiliesWithCode<TFamilyDto>(int code, CancellationToken cancellationToken = default)
            where TFamilyDto : class;
        Task<TFamilyDto?> GetByIdProjectedAsync<TFamilyDto>(int id, CancellationToken cancellationToken = default)
            where TFamilyDto : class;
        int SelectNextId();

        Task<PagedData<FamilyDto>> GetAllPaginatedTranslatedAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<FamilyDto?> GetByIdTranslatedAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FamilyDto>> SearchFamiliesWithCodeTranslatedAsync(int code, CancellationToken cancellationToken = default);
        Task<(string? Name, string? GuardianName)?> GetTranslatedNamesAsync(int id, string lang, CancellationToken cancellationToken = default);
        Task<Family?> GetByIdWithTranslationsAsync(int id, CancellationToken cancellationToken = default);
    }
}
