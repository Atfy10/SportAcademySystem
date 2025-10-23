using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;

namespace SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<bool>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<Result<bool>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EmployeeNotFoundException($"{request.Id}");

            cancellationToken.ThrowIfCancellationRequested();

            await _employeeRepository.DeleteAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operationType);
        }
    }
}
