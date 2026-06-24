using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;

public record ToggleUserActiveCommand(Guid UserId) : IRequest<Result<bool>>;
