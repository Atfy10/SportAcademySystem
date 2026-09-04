using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;
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
        private readonly IUserContextService _userContext;

        public CreateSalaryPaymentCommandHandler(
            ISalaryPaymentRepository repository,
            IEmployeeRepository employeeRepository,
            IUserContextService userContext)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _userContext = userContext;
        }

        public async Task<Result<SalaryPaymentDto>> Handle(CreateSalaryPaymentCommand request, CancellationToken cancellationToken)
        {
            // The salary payment's own BranchId always tracks the employee's current
            // employment branch at the moment it is filed - it isn't caller-supplied (unlike
            // Expense.BranchId), so there is no write-side branch-escape to guard against here.
            var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken)
                ?? throw new EmployeeNotFoundException(request.EmployeeId.ToString());

            var entity = new SalaryPayment
            {
                EmployeeId = request.EmployeeId,
                BranchId = employee.BranchId,
                Amount = request.Amount,
                PeriodMonth = request.PeriodMonth,
                PaymentTypeId = request.PaymentTypeId,
                Notes = request.Notes,
                Status = SalaryPaymentStatus.PendingApproval,
                CreatedByUserId = _userContext.UserId ?? Guid.Empty,
            };

            await _repository.AddAsync(entity, cancellationToken);

            var saved = await _repository.GetByIdWithIncludesAsync(entity.Id, cancellationToken) ?? entity;
            var nameLookup = new Dictionary<Guid, string>();

            return Result<SalaryPaymentDto>.Success(SalaryPaymentMapper.ToDto(saved, nameLookup), _operation);
        }
    }
}
