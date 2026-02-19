using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord
{
    public class GetCoachEmployeesWithoutCoachRecordQueryHandler : IRequestHandler<GetCoachEmployeesWithoutCoachRecordQuery, Result<PagedData<EmployeeDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetCoachEmployeesWithoutCoachRecordQueryHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedData<EmployeeDto>>> Handle(
            GetCoachEmployeesWithoutCoachRecordQuery request,
            CancellationToken cancellationToken)
        {
            var employeesDto = await _employeeRepository
                .GetCoachEmployeesWithoutCoachRecordAsync(request.Page, cancellationToken);

            return Result<PagedData<EmployeeDto>>.Success(employeesDto, _operationType);
        }
    }
}
