using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee
{
    public class CreateCoachWithEmployeeCommandHandler : IRequestHandler<CreateCoachWithEmployeeCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ICoachRepository _coachRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICoachBranchAccessRepository _coachBranchAccessRepository;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContext;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;

        public CreateCoachWithEmployeeCommandHandler(
            ICoachRepository coachRepository,
            IEmployeeRepository employeeRepository,
            ICoachBranchAccessRepository coachBranchAccessRepository,
            IMapper mapper,
            IUserContextService userContext,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _coachRepository = coachRepository;
            _employeeRepository = employeeRepository;
            _coachBranchAccessRepository = coachBranchAccessRepository;
            _mapper = mapper;
            _userContext = userContext;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateCoachWithEmployeeCommand request, CancellationToken ct)
        {
            var employee = _mapper.Map<Employee>(request.Employee)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            // Same rule as CreateCoachCommandHandler's existing-employee path: a Coach record
            // can only be created for a Coach-position employee, new or existing.
            if (employee.Position != Position.Coach)
                throw new EmployeeNotCoachPositionException(employee.Position.ToString());

            // SSN format/checksum is already enforced by CreateEmployeeDtoValidator
            // (ApplyNationalIdRuleFor, country-aware via IRegionalValidationService) in the
            // ValidationBehavior pipeline before this handler ever runs - a second, Kuwait-only
            // IsSSNValid gate here used to reject every non-Kuwait tenant's already-valid SSN.
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

            // See CreateCoachCommandHandler's identical call for why this is required - without
            // it this brand-new coach has zero CoachBranchAccess rows and never appears in the
            // group-creation coach picker at any branch, no matter how qualified.
            await _coachBranchAccessRepository.ReplaceForCoachAsync(
                coach.EmployeeId, coach.TenantId, [new CoachBranchAccess { BranchId = employee.BranchId }], ct);

            var actorName = _userContext.UserId is { } actorId
                ? await _userRepository.GetDisplayNameAsync(actorId, ct)
                : "System";
            await _publisher.Publish(
                new EmployeeLifecycleEvent(employee.Id, $"{employee.FirstName} {employee.LastName}", "Created as Coach", actorName),
                ct);

            return Result<int>.Success(coach.EmployeeId, _operationType);
        }
    }
}
