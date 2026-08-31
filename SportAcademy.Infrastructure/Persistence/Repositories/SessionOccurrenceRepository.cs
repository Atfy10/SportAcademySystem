using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;
using SportAcademy.Infrastructure.Persistence.Projections;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class SessionOccurrenceRepository : BaseRepository<SessionOccurrence, int>, ISessionOccurrenceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public SessionOccurrenceRepository(
            ApplicationDbContext context,
            IMapper mapper,
            ICurrentLanguageProvider languageProvider)
            : base(context, mapper, languageProvider)
        {
            _context = context;
            _mapper = mapper;
            _languageProvider = languageProvider;
        }

        public async Task<PagedData<SessionOccurrenceDto>> GetAllPaginatedAsync(PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .OrderByDescending(s => s.StartDateTime)
                .AsNoTracking()
                .Select(SessionOccurrenceProjections.ToDto(_languageProvider.Language));

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<PagedData<SessionOccurrenceDto>> GetByDateAsync(DateTime date, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .Where(s => s.StartDateTime.Date == date.Date)
                .OrderByDescending(s => s.StartDateTime)
                .AsNoTracking()
                .Select(SessionOccurrenceProjections.ToDto(_languageProvider.Language));

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<PagedData<SessionOccurrenceDto>> SearchAsync(string term, PageRequest page, CancellationToken cancellationToken = default)
        {
            var query = _context.SessionOccurrences
                .Where(s => s.GroupSchedule!.TraineeGroup!.Name.Contains(term)
                    || s.GroupSchedule.TraineeGroup.Coach!.Sport!.Name.Contains(term)
                    || (s.GroupSchedule.TraineeGroup.Coach.Employee!.FirstName + " " + s.GroupSchedule.TraineeGroup.Coach.Employee.LastName).Contains(term)
                    || s.GroupSchedule.TraineeGroup.Branch!.Name.Contains(term))
                .AsNoTracking()
                .Select(SessionOccurrenceProjections.ToDto(_languageProvider.Language));

            return await query.ToPagedDataAsync(page, cancellationToken);
        }

        public async Task<int?> GetTraineeGroupIdAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .Select(s => (int?)s.GroupSchedule!.TraineeGroupId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences.CountAsync(cancellationToken);

        public async Task<DateTime?> GetLastOccurrenceDateAsync(int traineeGroupId, CancellationToken cancellationToken = default)
            => await _context.SessionOccurrences
                .Where(s => s.GroupSchedule!.TraineeGroupId == traineeGroupId)
                .OrderByDescending(s => s.StartDateTime)
                .Select(s => (DateTime?)s.StartDateTime)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task AddRangeAsync(IEnumerable<SessionOccurrence> entities, CancellationToken cancellationToken = default)
        {
            await _context.SessionOccurrences.AddRangeAsync(entities, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
