using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.ExpireTenantSubscription;

public record ExpireTenantSubscriptionCommand(Guid TenantId) : IRequest<Result>;
