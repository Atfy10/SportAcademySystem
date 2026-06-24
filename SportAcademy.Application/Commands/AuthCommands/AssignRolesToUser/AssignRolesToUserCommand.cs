using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;

public record AssignRolesToUserCommand(Guid UserId, List<string> Roles) : IRequest<Result<bool>>;
