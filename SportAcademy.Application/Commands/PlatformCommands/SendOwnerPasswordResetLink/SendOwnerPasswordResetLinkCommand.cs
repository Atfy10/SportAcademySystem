using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.SendOwnerPasswordResetLink;

public record SendOwnerPasswordResetLinkCommand(Guid OwnerUserId) : IRequest<Result<bool>>;
