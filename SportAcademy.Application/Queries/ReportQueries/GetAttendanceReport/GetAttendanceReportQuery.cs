using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ReportDtos;

namespace SportAcademy.Application.Queries.ReportQueries.GetAttendanceReport;

// Status is a raw string (parsed to AttendanceStatus? in the handler) so an unrecognized/blank
// query-string value degrades to "no filter" instead of a 400 - matches how the rest of the
// report endpoints treat optional filters.
public record GetAttendanceReportQuery(
    DateTime? From, DateTime? To, int? BranchId, int? TraineeGroupId, int? TraineeId,
    int? CoachId, string? Status, PageRequest? Page)
    : IRequest<Result<PagedData<AttendanceSessionGroupDto>>>;
