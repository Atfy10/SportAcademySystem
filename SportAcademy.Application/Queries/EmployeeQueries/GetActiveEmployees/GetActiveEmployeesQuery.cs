using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees
{

    public record GetActiveEmployeesQuery
        : IRequest<Result<PagedData<EmployeeDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; }

        public GetActiveEmployeesQuery(PageRequest page)
        {
            Page = page;
        }
    }
}
