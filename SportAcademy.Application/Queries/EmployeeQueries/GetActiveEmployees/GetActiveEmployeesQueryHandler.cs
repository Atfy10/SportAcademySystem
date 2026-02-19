using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees
{
    public class GetActiveEmployeesQueryHandler : IRequestHandler<GetActiveEmployeesQuery, Result<PagedData<EmployeeDto>>>
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

        public async Task<Result<PagedData<EmployeeDto>>> Handle(
            GetActiveEmployeesQuery request,
            CancellationToken cancellationToken)
        {
            var employeesDto = await _employeeRepository
                .GetActiveAsync(request.Page, cancellationToken);

            return Result<PagedData<EmployeeDto>>.Success(employeesDto, _operationType);
        }
    }

}
