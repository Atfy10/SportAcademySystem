using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.TenantCommands.RecordFirstDashboardLoad;

public record RecordFirstDashboardLoadCommand(Guid TenantId, Guid UserId) : IRequest<Result>;
