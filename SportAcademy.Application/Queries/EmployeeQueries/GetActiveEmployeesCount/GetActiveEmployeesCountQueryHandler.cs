using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoachesCount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployeesCount
{
    public class GetActiveEmployeesCountQueryHandler : IRequestHandler<GetActiveEmployeesCountQuery, Result<int>>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetActiveEmployeesCountQueryHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<int>> Handle(GetActiveEmployeesCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _employeeRepository.GetActiveEmployeesCountAsync(cancellationToken);

            return Result<int>.Success(count, nameof(GetActiveEmployeesCountQuery));
        }
    }
}