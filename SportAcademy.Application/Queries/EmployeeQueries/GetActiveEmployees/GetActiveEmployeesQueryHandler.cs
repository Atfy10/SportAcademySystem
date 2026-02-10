using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees
{
    public class GetActiveEmployeesQueryHandler
    : IRequestHandler<GetActiveEmployeesQuery, Result<List<EmployeeDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetActiveEmployeesQueryHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<EmployeeDto>>> Handle(
            GetActiveEmployeesQuery request,
            CancellationToken cancellationToken)
        {
            var employees = await _employeeRepository
                .GetActiveAsync(cancellationToken);

            var employeesDto = _mapper.Map<List<EmployeeDto>>(employees)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<List<EmployeeDto>>.Success(employeesDto, _operationType);
        }
    }

}
