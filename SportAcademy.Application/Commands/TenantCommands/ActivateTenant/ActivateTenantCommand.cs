using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.TenantCommands.ActivateTenant;

public record ActivateTenantCommand(Guid TenantId) : IRequest<Result>;
