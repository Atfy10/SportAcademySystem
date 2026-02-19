using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.AttendanceDtos;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using SportAcademy.Infrastructure.Persistence.Extensions.QueryExtensions;

namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class AttendanceRepository : BaseRepository<Attendance, int>, IAttendanceRepository
    {
        private readonly ApplicationDbContext _context;
        IMapper _mapper;

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


        public async Task<PagedData<AttendanceDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default)
            => await _context.Attendances
                .Include(a => a.Enrollment)
                .Include(a => a.SessionOccurrence)
                .ProjectTo<AttendanceDto>(_mapper.ConfigurationProvider)
                .ToPagedDataAsync(page, cancellationToken);

        public async Task<(int TotalSessions, int AttendedSessions)> GetAttendanceSummaryAsync(
           int traineeId,
           DateOnly? fromDate,
           DateOnly? toDate,
           CancellationToken cancellationToken)
        {
            var query = _context.Attendances
                .Include(a => a.Enrollment)
                .Include(a => a.SessionOccurrence)
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
    }
}
