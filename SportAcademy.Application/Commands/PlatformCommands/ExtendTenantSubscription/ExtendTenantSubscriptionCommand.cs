using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PlatformCommands.ExtendTenantSubscription;

public record ExtendTenantSubscriptionCommand(Guid TenantId, int Days) : IRequest<Result>;
