using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IAttendanceRepository : IBaseRepository<Attendance, int>
    {
        Task<PagedData<AttendanceDto>> GetAllAsync(PageRequest page, CancellationToken cancellationToken = default);
        Task<int> GetGlobalAttendanceRate(Month month, CancellationToken ct = default);
        Task<(int TotalSessions, int AttendedSessions)> GetAttendanceSummaryAsync(
            int traineeId,
            DateOnly? fromDate,
            DateOnly? toDate,
            CancellationToken cancellationToken
        );
    }
}
