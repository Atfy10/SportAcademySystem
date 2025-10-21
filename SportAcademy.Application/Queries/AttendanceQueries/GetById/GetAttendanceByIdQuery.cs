using MediatR;
using SportAcademy.Application.DTOs.AttendanceDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetById
{
    public record GetAttendanceByIdQuery(int Id) : IRequest<Result<AttendanceDto>>;
}
