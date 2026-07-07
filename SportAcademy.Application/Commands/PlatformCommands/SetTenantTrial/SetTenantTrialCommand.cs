using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.SetTenantTrial;

public record SetTenantTrialCommand(Guid TenantId) : IRequest<Result>;
