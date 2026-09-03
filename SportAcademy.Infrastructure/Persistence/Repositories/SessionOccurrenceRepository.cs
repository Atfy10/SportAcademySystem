using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.SessionOccurrenceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
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

        public async Task<(int TraineeGroupId, DateTime StartDateTime, int DurationInMinutes)?> GetTimingAsync(
            int sessionOccurrenceId, CancellationToken cancellationToken = default)
        {
            var row = await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .Select(s => new
                {
                    TraineeGroupId = s.GroupSchedule!.TraineeGroupId,
                    s.StartDateTime,
                    DurationInMinutes = s.GroupSchedule!.TraineeGroup.DurationInMinutes
                })
                .FirstOrDefaultAsync(cancellationToken);

            return row is null ? null : (row.TraineeGroupId, row.StartDateTime, row.DurationInMinutes);
        }

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

        public async Task<int> SetFutureSessionsStatusAsync(
            int traineeGroupId, SessionStatus fromStatus, SessionStatus toStatus, DateTime asOf, CancellationToken cancellationToken = default)
        {
            var sessions = await _context.SessionOccurrences
                .Where(s => s.GroupSchedule!.TraineeGroupId == traineeGroupId
                    && s.Status == fromStatus
                    && s.StartDateTime > asOf)
                .ToListAsync(cancellationToken);

            foreach (var session in sessions)
                session.Status = toStatus;

            if (sessions.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return sessions.Count;
        }

        public async Task<List<SessionOccurrenceDto>> GetNearbyOccurrencesAsync(
            int sessionOccurrenceId, int pastCount, int futureCount, CancellationToken cancellationToken = default)
        {
            var target = await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .Select(s => new { s.GroupScheduleId, s.StartDateTime })
                .FirstOrDefaultAsync(cancellationToken);
            if (target is null) return [];

            var past = await _context.SessionOccurrences
                .Where(s => s.GroupScheduleId == target.GroupScheduleId && s.StartDateTime < target.StartDateTime)
                .OrderByDescending(s => s.StartDateTime)
                .Take(pastCount)
                .AsNoTracking()
                .Select(SessionOccurrenceProjections.ToDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

            var current = await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .AsNoTracking()
                .Select(SessionOccurrenceProjections.ToDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

            var future = await _context.SessionOccurrences
                .Where(s => s.GroupScheduleId == target.GroupScheduleId && s.StartDateTime > target.StartDateTime)
                .OrderBy(s => s.StartDateTime)
                .Take(futureCount)
                .AsNoTracking()
                .Select(SessionOccurrenceProjections.ToDto(_languageProvider.Language))
                .ToListAsync(cancellationToken);

            past.Reverse();
            return [.. past, .. current, .. future];
        }
    }
}
