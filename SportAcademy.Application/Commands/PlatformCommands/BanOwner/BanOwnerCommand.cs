using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.BanOwner;

public record BanOwnerCommand(Guid OwnerUserId, bool Banned) : IRequest<Result<bool>>;
