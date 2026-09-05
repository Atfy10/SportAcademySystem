using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.CreateSalaryPayment
{
    public class CreateSalaryPaymentCommandHandler : IRequestHandler<CreateSalaryPaymentCommand, Result<SalaryPaymentDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISalaryPaymentRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICoachBranchAccessRepository _coachBranchAccessRepository;
        private readonly IUserContextService _userContext;
        private readonly IPublisher _publisher;

        public CreateSalaryPaymentCommandHandler(
            ISalaryPaymentRepository repository,
            IEmployeeRepository employeeRepository,
            ICoachBranchAccessRepository coachBranchAccessRepository,
            IUserContextService userContext,
            IPublisher publisher)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _coachBranchAccessRepository = coachBranchAccessRepository;
            _userContext = userContext;
            _publisher = publisher;
        }

        public async Task<Result<SalaryPaymentDto>> Handle(CreateSalaryPaymentCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken)
                ?? throw new EmployeeNotFoundException(request.EmployeeId.ToString());

            // Default: the employee's own recorded (employment) branch, capped at their base
            // salary. If a BranchId is explicitly supplied, it must be either that recorded
            // branch or one of the coach's indicated (CoachBranchAccess) branches - using that
            // branch's own salary override when set, else falling back to the base salary.
            int resolvedBranchId = employee.BranchId;
            decimal cap = employee.Salary;

            if (request.BranchId.HasValue)
            {
                if (request.BranchId.Value == employee.BranchId)
                {
                    resolvedBranchId = employee.BranchId;
                    cap = employee.Salary;
                }
                else
                {
                    var branchAccess = await _coachBranchAccessRepository.GetForCoachAsync(employee.Id, cancellationToken);
                    var match = branchAccess.FirstOrDefault(a => a.BranchId == request.BranchId.Value);
                    if (match is null)
                        return Result<SalaryPaymentDto>.Failure(_operation, "Employee is not authorized for the selected branch.", 400);

                    resolvedBranchId = match.BranchId;
                    cap = match.Salary ?? employee.Salary;
                }
            }

            if (request.Amount > cap)
                return Result<SalaryPaymentDto>.Failure(_operation, "Amount exceeds the employee's base salary.", 400);

            var entity = new SalaryPayment
            {
                EmployeeId = request.EmployeeId,
                BranchId = resolvedBranchId,
                Amount = request.Amount,
                Bonus = request.Bonus ?? 0,
                PeriodMonth = request.PeriodMonth,
                PaymentTypeId = request.PaymentTypeId,
                Notes = request.Notes,
                Status = SalaryPaymentStatus.PendingApproval,
                CreatedByUserId = _userContext.UserId ?? Guid.Empty,
            };

            await _repository.AddAsync(entity, cancellationToken);
            await _publisher.Publish(new SalaryPaymentCreatedEvent(entity.Id), cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = new Dictionary<Guid, string>();

            return Result<SalaryPaymentDto>.Success(SalaryPaymentMapper.ToDto(saved, nameLookup), _operation);
        }
    }
}
