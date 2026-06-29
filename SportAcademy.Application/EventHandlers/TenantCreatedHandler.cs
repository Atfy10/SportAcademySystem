using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class TenantCreatedHandler : INotificationHandler<TenantCreatedEvent>
{
    private readonly ILogger<TenantCreatedHandler> _logger;

    public TenantCreatedHandler(ILogger<TenantCreatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TenantCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "TenantCreatedEvent: Tenant {TenantId} provisioned. Preparing provisioning workflow.",
            notification.TenantId);

        return Task.CompletedTask;
    }
}
