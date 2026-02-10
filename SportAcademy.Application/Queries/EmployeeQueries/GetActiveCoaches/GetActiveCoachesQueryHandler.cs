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

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches
{
    public class GetActiveCoachesQueryHandler
    : IRequestHandler<GetActiveCoachesQuery, Result<List<EmployeeDto>>>
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

        public async Task<Result<List<EmployeeDto>>> Handle(
            GetActiveCoachesQuery request,
            CancellationToken cancellationToken)
        {
            var coaches = await _employeeRepository
                .GetActiveCoachesAsync(cancellationToken);

            var coachesDto = _mapper.Map<List<EmployeeDto>>(coaches)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<List<EmployeeDto>>.Success(coachesDto, _operationType);
        }
    }

}
