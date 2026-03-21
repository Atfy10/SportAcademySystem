using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;

namespace SportAcademy.Application.Commands.EmployeeCommands.ToggleEmployeeStatus;

public class ToggleEmployeeStatusCommandHandler : IRequestHandler<ToggleEmployeeStatusCommand, Result<bool>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleEmployeeStatusCommandHandler(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<Result<bool>> Handle(ToggleEmployeeStatusCommand request, CancellationToken cancellationToken)
    {
        var newStatus = await _employeeRepository.ToggleIsWorkAsync(request.Id, cancellationToken);

        return Result<bool>.Success(newStatus, _operation);
    }
}
