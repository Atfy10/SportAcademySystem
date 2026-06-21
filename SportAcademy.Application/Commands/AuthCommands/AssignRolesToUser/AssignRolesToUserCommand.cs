using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;

public record AssignRolesToUserCommand(string UserId, List<string> Roles) : IRequest<Result<bool>>;
