using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus;

public record ChangeTenantStatusCommand(Guid TenantId, TenantStatus NewStatus) : IRequest<Result>;
