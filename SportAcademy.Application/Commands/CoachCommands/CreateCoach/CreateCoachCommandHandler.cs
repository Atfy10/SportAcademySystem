using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoach
{
    public class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ICoachRepository _coachRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IPersonService _personService;
        private readonly IMapper _mapper;

        public CreateCoachCommandHandler(
            IEmployeeRepository employeeRepository,
            ICoachRepository coachRepository,
            IPersonService personService,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _coachRepository = coachRepository;
            _personService = personService;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateCoachCommand request, CancellationToken ct)
        {
            var coach = _mapper.Map<Coach>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            ct.ThrowIfCancellationRequested();

            var employee = await _employeeRepository.GetByIdAsync(coach.EmployeeId, ct)
                ?? throw new IdNotFoundException(nameof(Employee), coach.EmployeeId.ToString());

            var isValid = _personService.IsSSNValid(coach.Employee.SSN, coach.Employee.BirthDate);
            if (!isValid)
                throw new SSNSyntaxErrorException();

            ct.ThrowIfCancellationRequested();

            var isSSNExist = await _employeeRepository.IsSSNExistAsync(employee.SSN, ct);
            if (isSSNExist)
                throw new SSNNotUniqueException();

            ct.ThrowIfCancellationRequested();

            await _coachRepository.AddAsync(coach, ct);

            ct.ThrowIfCancellationRequested();

            return Result<int>.Success(employee.Id, _operationType);
        }
    }
}
