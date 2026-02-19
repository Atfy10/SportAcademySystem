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

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches
{
    public class GetActiveCoachesQueryHandler : IRequestHandler<GetActiveCoachesQuery, Result<PagedData<EmployeeDto>>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetActiveCoachesQueryHandler(
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedData<EmployeeDto>>> Handle(
            GetActiveCoachesQuery request,
            CancellationToken cancellationToken)
        {
            var coachesDto = await _employeeRepository
                .GetActiveCoachesAsync(request.Page, cancellationToken);

            return Result<PagedData<EmployeeDto>>.Success(coachesDto, _operationType);
        }
    }

}
