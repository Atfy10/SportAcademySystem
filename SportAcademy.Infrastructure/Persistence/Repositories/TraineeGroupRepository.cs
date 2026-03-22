using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class TraineeGroupRepository : BaseRepository<TraineeGroup, int>, ITraineeGroupRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TraineeGroupRepository(ApplicationDbContext context, IMapper mapper)
            : base(context, mapper)
        {
            _mapper = mapper;
            _context = context;
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
                .ProjectTo<TraineeGroupCardDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<TraineeGroupDetailDto?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .Where(tg => tg.Id == id)
                .AsNoTracking()
                .ProjectTo<TraineeGroupDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<int> GetCountAsync(CancellationToken cancellation = default)
            => await _context.TraineeGroups.CountAsync(cancellation);

        public async Task<List<TraineeGroupDropdownDto>> GetAllForDropdownAsync(CancellationToken cancellationToken = default)
            => await _context.TraineeGroups
                .AsNoTracking()
                .ProjectTo<TraineeGroupDropdownDto>(_mapper.ConfigurationProvider)
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

