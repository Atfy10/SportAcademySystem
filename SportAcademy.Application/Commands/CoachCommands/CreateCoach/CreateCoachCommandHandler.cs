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
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoach
{
    public class CreateCoachCommandHandler : IRequestHandler<CreateCoachCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ICoachRepository _coachRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICoachBranchAccessRepository _coachBranchAccessRepository;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public CreateCoachCommandHandler(
            IEmployeeRepository employeeRepository,
            ICoachRepository coachRepository,
            ICoachBranchAccessRepository coachBranchAccessRepository,
            IPersonService personService,
            IMapper mapper,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _employeeRepository = employeeRepository;
            _coachRepository = coachRepository;
            _coachBranchAccessRepository = coachBranchAccessRepository;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateCoachCommand request, CancellationToken ct)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new EmployeeNotFoundException(request.EmployeeId.ToString());

            // An employee must already hold the Coach position before a Coach record can be
            // created for them - previously this silently overwrote whatever position they
            // actually held (HR, Manager, ...), which meant the wrong staff member could end up
            // teaching a sport with no one having deliberately decided that.
            if (employee.Position != Position.Coach)
                throw new EmployeeNotCoachPositionException(employee.Position.ToString());

            ct.ThrowIfCancellationRequested();

            // Coach shares its primary key with Employee 1:1 (see CoachConfiguration), so a prior
            // soft-deleted Coach row for this employee still occupies that key - inserting a new
            // row would hit a duplicate-key error. Recover it instead of failing.
            var existingCoach = await _coachRepository.GetByEmployeeIdIncludingDeletedAsync(request.EmployeeId, ct);

            Coach coach;
            if (existingCoach != null)
            {
                if (!existingCoach.IsDeleted)
                    throw new EmployeeAlreadyCoachException();

                existingCoach.RestoreFromDeleted();
                existingCoach.SportId = request.SportId;
                existingCoach.SkillLevel = request.SkillLevel;
                await _coachRepository.UpdateAsync(existingCoach, ct);
                coach = existingCoach;
            }
            else
            {
                coach = new Coach
                {
                    EmployeeId = request.EmployeeId,
                    SportId = request.SportId,
                    SkillLevel = request.SkillLevel
                };
                await _coachRepository.AddAsync(coach, ct);
            }

            // A coach is only assignable to a trainee group at branches they have explicit
            // CoachBranchAccess for (see that entity's own comment) - without this, a brand-new
            // coach has zero rows and silently never appears in the group-creation coach picker
            // at any branch, no matter how qualified. Seed the same single row the
            // AddCoachBranchAccess migration backfilled for pre-existing coaches: their
            // employment branch. An admin can grant more via "Manage branches" afterward.
            await _coachBranchAccessRepository.ReplaceForCoachAsync(
                coach.EmployeeId, coach.TenantId, [new CoachBranchAccess { BranchId = employee.BranchId }], ct);

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
