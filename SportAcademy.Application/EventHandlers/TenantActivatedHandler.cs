using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class TenantActivatedHandler : INotificationHandler<TenantActivatedEvent>
{
    private readonly ILogger<TenantActivatedHandler> _logger;

    public TenantActivatedHandler(ILogger<TenantActivatedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TenantActivatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "TenantActivatedEvent: Tenant {TenantId} transitioned from PendingSetup to Active at {ActivatedAt}.",
            notification.TenantId, DateTime.UtcNow);

        return Task.CompletedTask;
    }
}
