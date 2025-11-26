using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee
{
    public class CreateCoachWithEmployeeCommandHandler : IRequestHandler<CreateCoachWithEmployeeCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ICoachRepository _coachRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IMapper _mapper;
        private readonly IPersonService _personService;

        public CreateCoachWithEmployeeCommandHandler(
            ICoachRepository coachRepository,
            IEmployeeRepository employeeRepository,
            IMapper mapper,
            IPersonService personService)
        {
            _coachRepository = coachRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _personService = personService;
        }

        public async Task<Result<int>> Handle(CreateCoachWithEmployeeCommand request, CancellationToken ct)
        {
            var employee = _mapper.Map<Employee>(request.Employee)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            var isSSNValid = _personService.IsSSNValid(employee.SSN, employee.BirthDate);
            if (!isSSNValid)
                throw new SSNSyntaxErrorException();

            var isSSNExist = await _employeeRepository.IsSSNExistAsync(employee.SSN, ct);
            if (isSSNExist)
                throw new SSNNotUniqueException();

             //employee.AppUserId = "";

            ct.ThrowIfCancellationRequested();

            await _employeeRepository.AddAsync(employee, ct);

            var coach = _mapper.Map<Coach>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            coach.EmployeeId = employee.Id;

            ct.ThrowIfCancellationRequested();

            await _coachRepository.AddAsync(coach, ct);

            return Result<int>.Success(coach.EmployeeId, _operationType);
        }
    }
}
