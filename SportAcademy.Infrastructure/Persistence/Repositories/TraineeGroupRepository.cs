using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
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
                .ProjectTo<ListTraineeGroupDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<PagedData<TraineeGroupCardDto>> GetAllAsCardAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .AsNoTracking()
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

        public async Task<List<TraineeGroupDropdownDto>> GetAllForDropdownAsync(int? sportId = null, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Where(tg => sportId == null || tg.Coach.SportId == sportId.Value)
                .AsNoTracking()
                .Select(TraineeGroupProjections.ToDropdownDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

        public async Task<TraineeGroup?> GetByIdWithSchedulesAsync(int id, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Include(g => g.GroupSchedules)
                .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        public async Task<PagedData<ListTraineeGroupDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default)
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
                .ProjectTo<ListTraineeGroupDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);
        }
    }
}

