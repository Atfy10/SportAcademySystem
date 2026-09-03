using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;

namespace SportAcademy.Application.Commands.EmployeeCommands.ToggleEmployeeStatus;

public class ToggleEmployeeStatusCommandHandler : IRequestHandler<ToggleEmployeeStatusCommand, Result<bool>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUserContextService _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleEmployeeStatusCommandHandler(
        IEmployeeRepository employeeRepository,
        IUserContextService userContext,
        IUserRepository userRepository,
        IPublisher publisher)
    {
        _employeeRepository = employeeRepository;
        _userContext = userContext;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task<Result<bool>> Handle(ToggleEmployeeStatusCommand request, CancellationToken cancellationToken)
    {
        var newStatus = await _employeeRepository.ToggleIsWorkAsync(request.Id, cancellationToken);

        var actorName = _userContext.UserId is { } actorId
            ? await _userRepository.GetDisplayNameAsync(actorId, cancellationToken)
            : "System";
        await _publisher.Publish(new EmployeeStatusChangedEvent(request.Id, newStatus, actorName), cancellationToken);

        return Result<bool>.Success(newStatus, _operation);
    }
}
