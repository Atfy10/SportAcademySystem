using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Interfaces
{
    public interface ITraineeGroupRepository : IBaseRepository<TraineeGroup, int>
    {
        Task<int> GetCountAsync(CancellationToken cancellation = default);
        Task<PagedData<ListTraineeGroupDto>> GetAllOfSpecificDayAsync(PageRequest page, DateTime day, CancellationToken cancellationToken = default);
        Task<PagedData<TraineeGroupCardDto>> GetAllAsCardAsync(PageRequest page, TimeOnly? fromTime = null, TimeOnly? toTime = null, CancellationToken cancellationToken = default);
        Task<TraineeGroupDetailDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<TraineeGroupDropdownDto>> GetAllForDropdownAsync(int? sportId = null, Domain.Enums.SkillLevel? maxSkillLevel = null, Domain.Enums.Gender? gender = null, Domain.Enums.TraineeGroupType? groupType = null, IReadOnlyCollection<DayOfWeek>? trainingDays = null, CancellationToken cancellationToken = default);
        Task<TraineeGroup?> GetByIdWithSchedulesAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>
        /// The distinct weekly training-day patterns in use by active groups for this
        /// sport/branch (optionally narrowed to one group type), with how many groups run each.
        /// Drives the subscription form's day-pattern picker - a subscription's end date is
        /// counted across the pattern chosen here.
        /// </summary>
        Task<List<GroupDayPatternDto>> GetDayPatternsAsync(int sportId, int branchId, Domain.Enums.TraineeGroupType? groupType = null, CancellationToken cancellationToken = default);
        Task<PagedData<ListTraineeGroupDto>> SearchAsync(string term, PageRequest page, TimeOnly? fromTime = null, TimeOnly? toTime = null, CancellationToken cancellationToken = default);
        Task<int?> GetSportIdAsync(int traineeGroupId, CancellationToken cancellationToken = default);

        /// <summary>Tracked, with Translations eagerly loaded - for the Update handler to safely add/update/remove a translation row.</summary>
        Task<TraineeGroup?> GetByIdWithTranslationsAsync(int id, CancellationToken cancellationToken = default);

        /// <summary>The group's Name translated to <paramref name="lang"/>, or null if no translation row exists for that language.</summary>
        Task<string?> GetTranslatedNameAsync(int id, string lang, CancellationToken cancellationToken = default);

        /// <summary>The group's own Sport/Branch names translated to <paramref name="lang"/> - null per field if no translation row exists.</summary>
        Task<(string? SportName, string? BranchName)> GetTranslatedSportBranchNamesAsync(int id, string lang, CancellationToken cancellationToken = default);
    }
}
