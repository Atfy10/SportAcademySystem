using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SessionOccurrenceRepository : BaseRepository<SessionOccurrence, int>, ISessionOccurrenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SessionOccurrenceRepository(
            ApplicationDbContext context,
            IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedData<SessionOccurrenceDto>> GetAllPaginatedAsync(PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Coach)
                        .ThenInclude(c => c!.Employee)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Coach)
                        .ThenInclude(c => c!.Sport)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Branch)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Enrollments)
                .AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(s => s.StartDateTime)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(MapToDto).ToList();

            return new PagedData<SessionOccurrenceDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task<PagedData<SessionOccurrenceDto>> GetByDateAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Coach)
                        .ThenInclude(c => c!.Employee)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Coach)
                        .ThenInclude(c => c!.Sport)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Branch)
                .Include(s => s.GroupSchedule)
                    .ThenInclude(gs => gs.TraineeGroup!.Enrollments)
                .Where(s => s.StartDateTime.Date == date.Date)
                .AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(s => s.StartDateTime)
                .Skip((page.Page - 1) * page.PageSize)
                .Take(page.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = items.Select(MapToDto).ToList();

            return new PagedData<SessionOccurrenceDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task<PagedData<SessionOccurrenceDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .Where(s => s.GroupSchedule!.TraineeGroup!.Coach!.Sport!.Name.Contains(term)
                    || (s.GroupSchedule.TraineeGroup.Coach.Employee!.FirstName + " " + s.GroupSchedule.TraineeGroup.Coach.Employee.LastName).Contains(term)
                    || s.GroupSchedule.TraineeGroup.Branch!.Name.Contains(term))
                .AsNoTracking()
                .ProjectTo<SessionOccurrenceDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        private static SessionOccurrenceDto MapToDto(SessionOccurrence src)
        {
            var group = src.GroupSchedule!.TraineeGroup!;
            var enrollments = src.GroupSchedule!.TraineeGroup!.Enrollments.Where(e => e.IsActive).ToList();
            return new SessionOccurrenceDto(
                src.Id,
                group.Id,
                DateOnly.FromDateTime(src.StartDateTime),
                group.Coach!.Sport!.Name,
                $"{group.Coach.Employee!.FirstName} {group.Coach.Employee.LastName}",
                group.Branch!.Name,
                src.StartDateTime.ToString("HH:mm:ss"),
                group.DurationInMinutes,
                enrollments.Count,
                0,
                0,
                0
            );
        }

        public async Task<int?> GetTraineeGroupIdAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .Select(s => (int?)s.GroupSchedule.TraineeGroupId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences.CountAsync(cancellationToken);
    }
}
