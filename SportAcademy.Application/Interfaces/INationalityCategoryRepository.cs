using SportAcademy.Application.DTOs.NationalityCategoryDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface INationalityCategoryRepository : IBaseRepository<NationalityCategory, int>
    {
        /// <summary>All nationality categories with Name in the given language (falls back to English).</summary>
        Task<IReadOnlyList<NationalityCategoryDto>> GetAllTranslatedAsync(string lang, CancellationToken cancellationToken = default);

        /// <summary>True if another category already uses this Code (excluding <paramref name="excludeId"/>, for updates).</summary>
        Task<bool> IsCodeExistAsync(string code, int? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>True if another category already uses this Name (excluding <paramref name="excludeId"/>, for updates).</summary>
        Task<bool> IsNameExistAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>Tracked, with Translations eagerly loaded - for the Update handler to safely add/update/remove a translation row.</summary>
        Task<NationalityCategory?> GetByIdWithTranslationsAsync(int id, CancellationToken cancellationToken = default);
    }
}
