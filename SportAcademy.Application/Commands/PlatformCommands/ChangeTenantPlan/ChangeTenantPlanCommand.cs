using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;

public record ChangeTenantPlanCommand(Guid TenantId, int NewPlanId) : IRequest<Result>;
