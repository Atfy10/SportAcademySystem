using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AttendanceDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.AttendanceQueries.GetAll
{
    public class GetAttendancesQuery(PageRequest Page) 
        : IRequest<Result<PagedData<AttendanceDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}
