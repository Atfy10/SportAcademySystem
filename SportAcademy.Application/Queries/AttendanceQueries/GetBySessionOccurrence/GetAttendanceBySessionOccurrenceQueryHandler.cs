using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System.Net;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetBySessionOccurrence;

public class GetAttendanceBySessionOccurrenceQueryHandler(
    IAttendanceRepository attendanceRepository)
    : IRequestHandler<GetAttendanceBySessionOccurrenceQuery, Result<List<AttendanceRecordDto>>>
{
    public async Task<Result<List<AttendanceRecordDto>>> Handle(
        GetAttendanceBySessionOccurrenceQuery request,
        CancellationToken cancellationToken)
    {
        var records = await attendanceRepository.GetBySessionOccurrenceAsync(
            request.SessionOccurrenceId, cancellationToken);
        return Result<List<AttendanceRecordDto>>.Success(records, OperationType.Get.ToString());
    }
}
