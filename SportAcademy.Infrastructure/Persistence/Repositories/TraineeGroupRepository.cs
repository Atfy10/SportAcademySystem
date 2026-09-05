using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using SportAcademy.Infrastructure.Persistence.Projections;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeGroupRepository : BaseRepository<TraineeGroup, int>, ITraineeGroupRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public TraineeGroupRepository(ApplicationDbContext context, IMapper mapper, ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _mapper = mapper;
            _context = context;
            _languageProvider = languageProvider;
        }

        public async Task<PagedData<ListTraineeGroupDto>> GetAllOfSpecificDayAsync(PageRequest page, DateTime day, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Where(tg => tg.GroupSchedules.Any(gs => gs.Day == day.DayOfWeek))
                .AsNoTracking()
                .OrderBy(tg => tg.Id)
                .Select(TraineeGroupProjections.ToListDto(_languageProvider.Language))
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<PagedData<TraineeGroupCardDto>> GetAllAsCardAsync(PageRequest page, TimeOnly? fromTime = null, TimeOnly? toTime = null, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Where(tg => fromTime == null || toTime == null
                    || tg.GroupSchedules.Any(gs => fromTime.Value <= toTime.Value
                        ? gs.StartTime >= fromTime.Value && gs.StartTime < toTime.Value
                        : gs.StartTime >= fromTime.Value || gs.StartTime < toTime.Value))
                .AsNoTracking()
                .OrderBy(tg => tg.Id)
                .Select(TraineeGroupProjections.ToCardDto(_languageProvider.Language))
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<TraineeGroupDetailDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Where(tg => tg.Id == id)
                .AsNoTracking()
                .ProjectTo<TraineeGroupDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<int> GetCountAsync(CancellationToken cancellation = default)
            => await _context.TraineeGroups.CountAsync(cancellation);

        public async Task<int?> GetSportIdAsync(int traineeGroupId, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Where(tg => tg.Id == traineeGroupId)
                .Select(tg => (int?)tg.Coach.SportId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<List<TraineeGroupDropdownDto>> GetAllForDropdownAsync(
            int? sportId = null, SkillLevel? maxSkillLevel = null, Gender? gender = null,
            TraineeGroupType? groupType = null, IReadOnlyCollection<DayOfWeek>? trainingDays = null,
            CancellationToken cancellationToken = default)
        {
            // The trainee's own Gender (Male/Female) maps onto the group's TraineeGroupGender
            // policy (Male/Female/Mixed) by name, not by underlying numeric value.
            TraineeGroupGender? traineeAsGroupGender = gender switch
            {
                Gender.Male => TraineeGroupGender.Male,
                Gender.Female => TraineeGroupGender.Female,
                _ => null
            };

            // sportId/gender filter in SQL (both are equality checks against HasConversion<string>
            // columns, which EF translates correctly). SkillLevel does NOT: comparing "at or below
            // maxSkillLevel" is an ordinal comparison, and EF would either fail to translate it
            // against a converted column or - worse - silently translate it into a lexicographic
            // string comparison that doesn't match the enum's declared order ("Advanced" sorts
            // before "Beginner" alphabetically). ToDropdownDto already projects SkillLevel back to
            // its real enum type, so filtering by it after materializing is a plain in-memory
            // int comparison instead - no translation involved, so no ordering bug possible.
            var items = await _context.TraineeGroups
                // A paused group isn't accepting new enrollments - never offer it here.
                .Where(tg => tg.IsActive)
                .Where(tg => sportId == null || tg.Coach.SportId == sportId.Value)
                .Where(tg => traineeAsGroupGender == null
                    || tg.Gender == TraineeGroupGender.Mixed
                    || tg.Gender == traineeAsGroupGender.Value)
                .Where(tg => groupType == null || tg.Type == groupType.Value)
                .AsNoTracking()
                .Select(TraineeGroupProjections.ToDropdownDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

            if (maxSkillLevel.HasValue)
                items = items.Where(i => i.SkillLevel <= maxSkillLevel.Value).ToList();

            // Set equality against the subscription's chosen pattern, done in memory for the same
            // reason SkillLevel is above: this is a whole-set comparison, not something EF can
            // translate cleanly, and the candidate list here is one sport's groups at most.
            // Exact match (not "contains") matters - the subscription's end date was counted
            // across precisely these days, so a group training on more or fewer of them would
            // finish on a different date than the trainee was billed for.
            if (trainingDays is { Count: > 0 })
            {
                var wanted = trainingDays.ToHashSet();
                items = items.Where(i => wanted.SetEquals(i.TrainingDays)).ToList();
            }

            return items;
        }

        public async Task<TraineeGroup?> GetByIdWithSchedulesAsync(int id, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Include(g => g.GroupSchedules)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        // The distinct weekly day-patterns actually being run for this sport/branch/type - what
        // a subscription picks from, since its end date is counted across whichever pattern the
        // trainee will train on. Offering patterns that exist rather than free-form day picking
        // is what guarantees a matching group exists once it's time to assign one.
        public async Task<List<GroupDayPatternDto>> GetDayPatternsAsync(
            int sportId, int branchId, TraineeGroupType? groupType = null, CancellationToken cancellationToken = default)
        {
            var schedules = await _context.TraineeGroups
                .Where(tg => tg.IsActive)
                .Where(tg => tg.Coach.SportId == sportId)
                .Where(tg => tg.BranchId == branchId)
                .Where(tg => groupType == null || tg.Type == groupType.Value)
                .Where(tg => tg.GroupSchedules.Any())
                .AsNoTracking()
                .Select(tg => new
                {
                    tg.Type,
                    Days = tg.GroupSchedules.Select(gs => gs.Day).Distinct().ToList()
                })
                .ToListAsync(cancellationToken);

            // Grouped in memory: the key is a whole set of days, which SQL has no way to group by.
            return schedules
                .GroupBy(s => string.Join(",", s.Days.OrderBy(d => d).Select(d => (int)d)))
                .Select(g => new GroupDayPatternDto(
                    g.First().Days.OrderBy(d => d).ToList(),
                    g.Count()))
                .OrderByDescending(p => p.GroupCount)
                .ThenBy(p => p.Days.Count)
                .ToList();
        }

        public async Task<TraineeGroup?> GetByIdWithTranslationsAsync(int id, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Include(g => g.Translations)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        public async Task<string?> GetTranslatedNameAsync(int id, string lang, CancellationToken cancellationToken = default)
            => await _context.TraineeGroupTranslations
                .Where(t => t.TraineeGroupId == id && t.LangCode == lang)
                .Select(t => t.Name)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<(string? SportName, string? BranchName)> GetTranslatedSportBranchNamesAsync(int id, string lang, CancellationToken cancellationToken = default)
        {
            var result = await _context.TraineeGroups
                .Where(g => g.Id == id)
                .Select(g => new
                {
                    SportName = g.Coach.Sport.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault(),
                    BranchName = g.Branch.Translations.Where(t => t.LangCode == lang).Select(t => t.Name).FirstOrDefault(),
                })
                .FirstOrDefaultAsync(cancellationToken);

            return (result?.SportName, result?.BranchName);
        }

        public async Task<PagedData<ListTraineeGroupDto>> SearchAsync(string term, PageRequest page, TimeOnly? fromTime = null, TimeOnly? toTime = null, CancellationToken cancellationToken = default)
        {
            var lowerTerm = term.ToLower();
            return await _context.TraineeGroups
                .AsNoTracking()
                .Include(g => g.Coach)
                    .ThenInclude(c => c.Employee)
                .Include(g => g.Coach)
                    .ThenInclude(c => c.Sport)
                .Include(g => g.Branch)
                .Include(g => g.GroupSchedules)
                .Where(g =>
                    g.Coach.Sport.Name.ToLower().Contains(lowerTerm) ||
                    (g.Coach.Employee.FirstName + " " + g.Coach.Employee.LastName).ToLower().Contains(lowerTerm) ||
                    g.Branch.Name.ToLower().Contains(lowerTerm) ||
                    g.Name.ToLower().Contains(lowerTerm))
                .Where(g => fromTime == null || toTime == null
                    || g.GroupSchedules.Any(gs => fromTime.Value <= toTime.Value
                        ? gs.StartTime >= fromTime.Value && gs.StartTime < toTime.Value
                        : gs.StartTime >= fromTime.Value || gs.StartTime < toTime.Value))
                .OrderBy(g => g.Id)
                .Select(TraineeGroupProjections.ToListDto(_languageProvider.Language))
                .ToPagedDataAsync(page, cancellationToken);
        }
    }
}

