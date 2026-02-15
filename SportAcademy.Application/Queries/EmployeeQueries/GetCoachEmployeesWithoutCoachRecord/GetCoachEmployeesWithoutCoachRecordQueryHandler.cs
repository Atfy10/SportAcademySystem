using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord
{
    public class GetCoachEmployeesWithoutCoachRecordQueryHandler : IRequestHandler<GetCoachEmployeesWithoutCoachRecordQuery, Result<List<EmployeeDto>>>
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

        public async Task<Result<List<EmployeeDto>>> Handle(
            GetCoachEmployeesWithoutCoachRecordQuery request,
            CancellationToken cancellationToken)
        {
            var employees = await _employeeRepository
                .GetCoachEmployeesWithoutCoachRecordAsync(cancellationToken) ?? [];

            var employeesDto = _mapper.Map<List<EmployeeDto>>(employees)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<List<EmployeeDto>>.Success(employeesDto, _operationType);
        }
    }
}
