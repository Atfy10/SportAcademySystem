using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
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

        public CreateCoachWithEmployeeCommandHandler(
            ICoachRepository coachRepository,
            IEmployeeRepository employeeRepository,
            IMapper mapper)
        {
            _coachRepository = coachRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateCoachWithEmployeeCommand request, CancellationToken ct)
        {
            var employee = _mapper.Map<Employee>(request.Employee)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            if (!Person.IsSsnValid(employee.SSN, employee.BirthDate))
                throw new SSNSyntaxErrorException();

            var isSSNExist = await _employeeRepository.IsSSNExistAsync(employee.SSN, ct);
            if (isSSNExist)
                throw new SSNNotUniqueException();

            ct.ThrowIfCancellationRequested();

            await _employeeRepository.AddAsync(employee, ct);

            var coach = request.ToCoach(employee.Id);

            ct.ThrowIfCancellationRequested();

            await _coachRepository.AddAsync(coach, ct);

            return Result<int>.Success(coach.EmployeeId, _operationType);
        }
    }
}
