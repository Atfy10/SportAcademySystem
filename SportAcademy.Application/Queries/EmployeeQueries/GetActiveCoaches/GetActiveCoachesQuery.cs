using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches
{
    using MediatR;
    using SportAcademy.Application.Common.Result;
    using SportAcademy.Application.DTOs.EmployeeDtos;

    public record GetActiveCoachesQuery() : IRequest<Result<List<EmployeeDto>>>;

}
