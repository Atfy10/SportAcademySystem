using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.DTOs.ReportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class AttendanceRepository : BaseRepository<Attendance, int>, IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AttendanceRepository(ApplicationDbContext context, IMapper mapper)
            : base(context, mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<int> GetGlobalAttendanceRate(CancellationToken ct = default)
            => await _context.Attendances
                .CountAsync(a => a.AttendanceStatus == AttendanceStatus.Present, ct) * 100 /
                await _context.Attendances.CountAsync(ct);

        public async Task<int> GetMonthlyAttendanceRate(Month month, int? year, CancellationToken ct = default)
        {
            var query = _context.Attendances.Where(a => a.AttendanceDate.Month == (int)month);
            if (year.HasValue)
            {
                query = query.Where(a => a.AttendanceDate.Year == year.Value);
            }

            var total = await query.CountAsync(ct);
            var present = await query.CountAsync(a => a.AttendanceStatus == AttendanceStatus.Present, ct);
            return present * 100 / total;
        }

        public async Task<PagedData<AttendanceDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Attendances
                .AsNoTracking()
                .ProjectTo<AttendanceDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<(int TotalSessions, int AttendedSessions)> GetAttendanceSummaryAsync(
           int traineeId,
           DateOnly? fromDate,
           DateOnly? toDate,
           CancellationToken cancellationToken)
        {
            var query = _context.Attendances
                .Where(a => a.Enrollment.TraineeId == traineeId);

            if (fromDate.HasValue)
                query = query.Where(a => 
                    DateOnly.FromDateTime(a.SessionOccurrence.StartDateTime) >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => 
                    DateOnly.FromDateTime(a.SessionOccurrence.StartDateTime) <= toDate.Value);

            var total = await query.CountAsync(cancellationToken);
            var attended = await query
                .CountAsync(a => a.AttendanceStatus == AttendanceStatus.Present, cancellationToken);

            return (total, attended);
        }

        // The roster to mark is the trainee group's active enrollments, not the Attendances
        // table - a session that has never been marked yet has zero Attendance rows, and
        // querying Attendances directly (the previous implementation) returned an empty roster
        // for every unmarked session instead of the trainees waiting to be marked. Existing
        // Attendance rows (already marked/re-marked) are merged in below; enrollments with none
        // yet default to Absent, matching the frontend's own default for an unmarked trainee.
        public async Task<List<AttendanceRecordDto>> GetBySessionOccurrenceAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default)
        {
            var traineeGroupId = await _context.SessionOccurrences
                .Where(s => s.Id == sessionOccurrenceId)
                .Select(s => (int?)s.GroupSchedule!.TraineeGroupId)
                .FirstOrDefaultAsync(cancellationToken);

            if (traineeGroupId == null) return [];

            var roster = await _context.Enrollments
                .Where(e => e.TraineeGroupId == traineeGroupId.Value && e.IsActive)
                .Select(e => new { e.TraineeId, TraineeName = e.Trainee.FirstName + " " + e.Trainee.LastName })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var existing = await _context.Attendances
                .Where(a => a.SessionOccurrenceId == sessionOccurrenceId)
                .Select(a => new { a.Id, a.Enrollment.TraineeId, a.CheckInTime, a.AttendanceStatus })
                .AsNoTracking()
                .ToDictionaryAsync(a => a.TraineeId, cancellationToken);

            return roster
                .Select(r => existing.TryGetValue(r.TraineeId, out var a)
                    ? new AttendanceRecordDto(a.Id, r.TraineeId, r.TraineeName, a.CheckInTime.ToString("HH:mm:ss"), a.AttendanceStatus.ToString())
                    : new AttendanceRecordDto(0, r.TraineeId, r.TraineeName, null, AttendanceStatus.Absent.ToString()))
                .ToList();
        }

        public async Task<Attendance?> GetBySessionAndTraineeAsync(int sessionOccurrenceId, int traineeId, CancellationToken cancellationToken = default)
            => await _context.Attendances
                .Where(a => a.SessionOccurrenceId == sessionOccurrenceId && a.Enrollment.TraineeId == traineeId)
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<PagedData<AttendanceSessionGroupDto>> GetReportAsync(
            DateTime? from, DateTime? to, int? branchId, int? traineeGroupId, int? traineeId,
            int? coachId, AttendanceStatus? status, PageRequest? page, CancellationToken ct = default)
        {
            IQueryable<Attendance> query = _context.Attendances
                .Include(a => a.Enrollment).ThenInclude(e => e.Trainee)
                .Include(a => a.Enrollment).ThenInclude(e => e.TraineeGroup).ThenInclude(g => g.Branch)
                .Include(a => a.Enrollment).ThenInclude(e => e.TraineeGroup).ThenInclude(g => g.Coach).ThenInclude(c => c.Employee)
                .AsNoTracking();

            if (from.HasValue) query = query.Where(a => a.AttendanceDate >= from.Value);
            if (to.HasValue) query = query.Where(a => a.AttendanceDate <= to.Value);
            if (branchId.HasValue) query = query.Where(a => a.Enrollment.TraineeGroup.BranchId == branchId.Value);
            if (traineeGroupId.HasValue) query = query.Where(a => a.Enrollment.TraineeGroupId == traineeGroupId.Value);
            if (traineeId.HasValue) query = query.Where(a => a.Enrollment.TraineeId == traineeId.Value);
            if (coachId.HasValue) query = query.Where(a => a.Enrollment.TraineeGroup.CoachId == coachId.Value);
            if (status.HasValue) query = query.Where(a => a.AttendanceStatus == status.Value);

            // Pagination is over sessions (distinct TraineeGroup+AttendanceDate pairs), not
            // individual attendance rows - a page must never split one session's trainee list
            // across two pages. GroupBy with a nested per-group collection projection doesn't
            // reliably translate to SQL (see GetLatestSubscriptionsAsync's own note on this same
            // limitation), so this uses the same "keys first, then re-query and group in memory"
            // pattern already established there.
            var sessionKeysQuery = query
                .Select(a => new { TraineeGroupId = a.Enrollment.TraineeGroupId, a.AttendanceDate })
                .Distinct();

            var totalCount = await sessionKeysQuery.CountAsync(ct);
            var take = page?.PageSize ?? 5000;

            var pageKeys = await sessionKeysQuery
                .OrderByDescending(k => k.AttendanceDate)
                .Skip(page?.Skip ?? 0)
                .Take(take)
                .ToListAsync(ct);

            if (pageKeys.Count == 0)
            {
                return new PagedData<AttendanceSessionGroupDto>
                {
                    Items = [],
                    TotalCount = totalCount,
                    Page = page?.Page ?? 1,
                    PageSize = take,
                };
            }

            var groupIds = pageKeys.Select(k => k.TraineeGroupId).Distinct().ToList();
            var dates = pageKeys.Select(k => k.AttendanceDate).Distinct().ToList();

            var rows = await query
                .Where(a => groupIds.Contains(a.Enrollment.TraineeGroupId) && dates.Contains(a.AttendanceDate))
                .Select(a => new
                {
                    a.Id,
                    a.AttendanceDate,
                    Status = a.AttendanceStatus.ToString(),
                    CheckInTime = a.CheckInTime.ToString(),
                    a.CoachNote,
                    a.Enrollment.TraineeId,
                    TraineeName = a.Enrollment.Trainee.FirstName + " " + a.Enrollment.Trainee.LastName,
                    TraineeGroupId = a.Enrollment.TraineeGroupId,
                    TraineeGroupName = a.Enrollment.TraineeGroup.Name,
                    BranchName = a.Enrollment.TraineeGroup.Branch.Name,
                    CoachName = a.Enrollment.TraineeGroup.Coach.Employee.FirstName + " " + a.Enrollment.TraineeGroup.Coach.Employee.LastName,
                })
                .ToListAsync(ct);

            // groupIds x dates is a superset of this page's actual (group, date) pairs - narrow
            // back down to exactly pageKeys before grouping, then group in memory (already a
            // small, page-bounded set of rows at this point).
            var keySet = pageKeys.Select(k => (k.TraineeGroupId, k.AttendanceDate)).ToHashSet();

            var groups = rows
                .Where(r => keySet.Contains((r.TraineeGroupId, r.AttendanceDate)))
                .GroupBy(r => (r.TraineeGroupId, r.AttendanceDate.Date))
                .Select(g => new AttendanceSessionGroupDto(
                    g.Key.TraineeGroupId,
                    g.First().TraineeGroupName,
                    g.First().BranchName,
                    g.First().CoachName,
                    g.Key.Date,
                    g.Select(r => new AttendanceReportRowDto(r.Id, r.Status, r.CheckInTime, r.CoachNote, r.TraineeId, r.TraineeName))
                        .ToList()))
                .OrderByDescending(g => g.AttendanceDate)
                .ToList();

            return new PagedData<AttendanceSessionGroupDto>
            {
                Items = groups,
                TotalCount = totalCount,
                Page = page?.Page ?? 1,
                PageSize = take,
            };
        }
    }
}
