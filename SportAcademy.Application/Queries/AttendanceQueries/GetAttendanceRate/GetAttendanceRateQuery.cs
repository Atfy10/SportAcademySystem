using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetAttendanceRate
{

    public record GetAttendanceRateQuery(
        int TraineeId,
        DateOnly? FromDate,
        DateOnly? ToDate
    ) : IRequest<Result<AttendanceRateDto>>;

}
