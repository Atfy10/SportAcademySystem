using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetAll
{
    public class GetAttendancesQuery() : IRequest<Result<List<AttendanceDto>>>;
}
