using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoach
{
    public class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ICoachRepository _coachRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public CreateCoachCommandHandler(
            IEmployeeRepository employeeRepository,
            ICoachRepository coachRepository,
            IPersonService personService,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _coachRepository = coachRepository;
        }

        public async Task<Result<int>> Handle(CreateCoachCommand request, CancellationToken ct)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new EmployeeNotFoundException(request.EmployeeId.ToString());

            ct.ThrowIfCancellationRequested();

            var coach = new Coach
            {
                EmployeeId = request.EmployeeId,
                SportId = request.SportId,
                SkillLevel = request.SkillLevel
            };

            ct.ThrowIfCancellationRequested();

            await _coachRepository.AddAsync(coach, ct);

            return Result<int>.Success(employee.Id, _operationType);
        }
    }
}
