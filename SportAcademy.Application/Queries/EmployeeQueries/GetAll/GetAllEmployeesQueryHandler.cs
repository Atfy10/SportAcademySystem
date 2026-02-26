using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetAll
{
    public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, Result<PagedData<EmployeeCardDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllEmployeesQueryHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<PagedData<EmployeeCardDto>>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
        {
            var employeesDto = await _employeeRepository.GetAllAsync(request.Page, cancellationToken);

            return Result<PagedData<EmployeeCardDto>>.Success(employeesDto, _operationType);
        }
    }
}
