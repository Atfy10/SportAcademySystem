using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ReportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.ReportQueries.GetAttendanceReport;

public class GetAttendanceReportQueryHandler : IRequestHandler<GetAttendanceReportQuery, Result<PagedData<AttendanceSessionGroupDto>>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetAttendanceReportQueryHandler(IAttendanceRepository attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<Result<PagedData<AttendanceSessionGroupDto>>> Handle(GetAttendanceReportQuery request, CancellationToken ct)
    {
        Enum.TryParse<AttendanceStatus>(request.Status, ignoreCase: true, out var parsedStatus);
        AttendanceStatus? status = string.IsNullOrWhiteSpace(request.Status) ? null : parsedStatus;

        var data = await _attendanceRepository.GetReportAsync(
            request.From, request.To, request.BranchId, request.TraineeGroupId, request.TraineeId,
            request.CoachId, status, request.Page, ct);

        return Result<PagedData<AttendanceSessionGroupDto>>.Success(data, _operation);
    }
}
