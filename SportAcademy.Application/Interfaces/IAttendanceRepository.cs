using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.DTOs.ReportDtos;
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
        Task<int> GetMonthlyAttendanceRate(Month month, int? year, CancellationToken ct = default);
        Task<int> GetGlobalAttendanceRate(CancellationToken ct = default);
        Task<(int TotalSessions, int AttendedSessions)> GetAttendanceSummaryAsync(
            int traineeId,
            DateOnly? fromDate,
            DateOnly? toDate,
            CancellationToken cancellationToken
        );
        Task<List<AttendanceRecordDto>> GetBySessionOccurrenceAsync(int sessionOccurrenceId, CancellationToken cancellationToken = default);
        Task<Attendance?> GetBySessionAndTraineeAsync(int sessionOccurrenceId, int traineeId, CancellationToken cancellationToken = default);
        Task<PagedData<AttendanceSessionGroupDto>> GetReportAsync(
            DateTime? from, DateTime? to, int? branchId, int? traineeGroupId, int? traineeId,
            int? coachId, AttendanceStatus? status, PageRequest? page, CancellationToken ct = default);
    }
}
