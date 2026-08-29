using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface INationalityCategoryRepository : IBaseRepository<NationalityCategory, int>
    {
        /// <summary>All nationality categories with Name in the given language (falls back to English).</summary>
        Task<IReadOnlyList<NationalityCategoryDto>> GetAllTranslatedAsync(string lang, CancellationToken cancellationToken = default);
    }
}
