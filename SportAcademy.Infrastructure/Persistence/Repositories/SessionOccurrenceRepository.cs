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
                .OrderByDescending(s => s.StartDateTime)
                .AsNoTracking()
                .ProjectTo<SessionOccurrenceDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<PagedData<SessionOccurrenceDto>> GetByDateAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .Where(s => s.StartDateTime.Date == date.Date)
                .OrderByDescending(s => s.StartDateTime)
                .AsNoTracking()
                .ProjectTo<SessionOccurrenceDto>(_mapper.ConfigurationProvider);

            return await query.ToPagedDataAsync(page, cancellationToken);
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

        public async Task<int?> GetTraineeGroupIdAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .Select(s => (int?)s.GroupSchedule!.TraineeGroupId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences.CountAsync(cancellationToken);
    }
}
