using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee
{
    public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<bool>>
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserContextService _userContextService;
        private readonly IUserRepository _userRepository;
        private readonly IPublisher _publisher;
        private readonly string _operationType = OperationType.Delete.ToString();

        public DeleteEmployeeCommandHandler(
            IEmployeeRepository employeeRepository,
            IUserContextService userContextService,
            IUserRepository userRepository,
            IPublisher publisher)
        {
            _employeeRepository = employeeRepository;
            _userContextService = userContextService;
            _userRepository = userRepository;
            _publisher = publisher;
        }

        public async Task<Result<bool>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _employeeRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new EmployeeNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            employee.MarkAsDeleted(_userContextService.UserId.ToString() ?? "System");
            await _employeeRepository.UpdateAsync(employee, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var actorName = _userContextService.UserId is { } actorId
                ? await _userRepository.GetDisplayNameAsync(actorId, cancellationToken)
                : "System";
            await _publisher.Publish(
                new EmployeeLifecycleEvent(employee.Id, $"{employee.FirstName} {employee.LastName}", "Deleted", actorName),
                cancellationToken);

            return Result<bool>.Success(true, _operationType);
        }
    }
}
