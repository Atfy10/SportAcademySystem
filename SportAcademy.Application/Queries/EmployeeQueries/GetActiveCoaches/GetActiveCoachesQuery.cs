using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches
{
    using MediatR;
    using SportAcademy.Application.DTOs.EmployeeDtos;
    using SportAcademy.Application.Services;

    public record GetActiveCoachesQuery()
        : IRequest<Result<List<EmployeeDto>>>;

}
