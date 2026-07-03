using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant;

public record ArchiveTenantCommand(Guid TenantId) : IRequest<Result>;
