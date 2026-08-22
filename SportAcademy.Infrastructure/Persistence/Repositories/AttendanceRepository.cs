using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.AttendanceDtos;
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
    }
}
