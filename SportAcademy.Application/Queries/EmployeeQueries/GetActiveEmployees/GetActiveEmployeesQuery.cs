using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using MediatR;
    using SportAcademy.Application.DTOs.EmployeeDtos;
    using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees
{

    public record GetActiveEmployeesQuery() : IRequest<Result<List<EmployeeDto>>>;
}
