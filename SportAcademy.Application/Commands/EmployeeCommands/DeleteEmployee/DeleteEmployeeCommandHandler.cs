using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<bool>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserContextService _userContextService;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IUserContextService userContextService)
        {
            _employeeRepository = employeeRepository;
            _userContextService = userContextService;
        }

        public async Task<Result<bool>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EmployeeNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            // Soft delete instead of hard delete
            employee.MarkAsDeleted(_userContextService.UserId ?? "System");
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operationType);
        }
    }
}
