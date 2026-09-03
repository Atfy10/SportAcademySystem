using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
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
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public CreateCoachCommandHandler(
            IEmployeeRepository employeeRepository,
            ICoachRepository coachRepository,
            IPersonService personService,
            IMapper mapper,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _employeeRepository = employeeRepository;
            _coachRepository = coachRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
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

            employee.Position = Position.Coach;

            ct.ThrowIfCancellationRequested();

            await _coachRepository.AddAsync(coach, ct);

            var actorName = _userContext.UserId is { } actorId
                ? await _userRepository.GetDisplayNameAsync(actorId, ct)
                : "System";
            await _publisher.Publish(
                new EmployeeLifecycleEvent(employee.Id, $"{employee.FirstName} {employee.LastName}", "Assigned as Coach", actorName),
                ct);

            return Result<int>.Success(employee.Id, _operationType);
        }
    }
}
