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

    public record GetActiveEmployeesQuery(PageRequest Page)
        : IRequest<Result<PagedData<EmployeeDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; }= Page;
    }
}
