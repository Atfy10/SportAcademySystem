using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.EmployeeCommands.ToggleEmployeeStatus;

public record ToggleEmployeeStatusCommand(int Id) : IRequest<Result<bool>>;
