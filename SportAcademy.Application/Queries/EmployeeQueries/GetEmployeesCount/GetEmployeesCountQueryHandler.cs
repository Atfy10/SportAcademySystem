using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetEmployeesCount
{
    public class GetEmployeesCountQueryHandler : IRequestHandler<GetEmployeesCountQuery, Result<int>>
    {
        private readonly IEmployeeRepository _employeeRepository;

        public GetEmployeesCountQueryHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<int>> Handle(GetEmployeesCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _employeeRepository.GetEmployeesCountAsync(cancellationToken);

            return Result<int>.Success(count, nameof(GetEmployeesCountQuery));
        }
    }
}
