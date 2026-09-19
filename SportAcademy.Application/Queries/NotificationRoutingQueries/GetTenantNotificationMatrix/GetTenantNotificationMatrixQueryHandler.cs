using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.Queries.NotificationRoutingQueries.GetTenantNotificationMatrix;

public class GetTenantNotificationMatrixQueryHandler
    : IRequestHandler<GetTenantNotificationMatrixQuery, Result<List<NotificationMatrixCellDto>>>
{
    private static readonly NotificationChannel[] AllChannels =
        [NotificationChannel.InApp, NotificationChannel.Email, NotificationChannel.Push, NotificationChannel.WhatsApp];

    private readonly INotificationSettingsRepository _repository;
    private readonly IUserContextService _userContext;

    public GetTenantNotificationMatrixQueryHandler(
        INotificationSettingsRepository repository, IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<List<NotificationMatrixCellDto>>> Handle(
        GetTenantNotificationMatrixQuery request, CancellationToken cancellationToken)
    {
        var tenantId = _userContext.TenantId
            ?? throw new InvalidOperationException("GetTenantNotificationMatrixQuery invoked without a resolved tenant context.");

        var eventTypes = await _repository.GetAllEventTypesAsync(cancellationToken);
        var rules = await _repository.GetTenantRulesAsync(tenantId, cancellationToken);
        var implementedQueuedChannels = await _repository.GetImplementedQueuedChannelsAsync(cancellationToken);

        var cells = new List<NotificationMatrixCellDto>();
        foreach (var eventType in eventTypes)
        {
            foreach (var channel in AllChannels)
            {
                var hasRule = rules.TryGetValue((eventType.Id, channel), out var explicitEnabled);
                var isEnabled = channel switch
                {
                    // Hardcoded, never a matrix lookup - see NotificationChannelDispatcher's
                    // identical reasoning.
                    NotificationChannel.InApp => true,
                    _ when hasRule => explicitEnabled,
                    // Same defaults as NotificationChannelDispatcher/AppDataSeeder's
                    // ReconcileTenantNotificationChannelRulesAsync - Push rides along with every
                    // InApp/SignalR push by default, Email only for the narrower set.
                    NotificationChannel.Email => NotificationEventTypes.DefaultEmailOnKeys.Contains(eventType.Key),
                    NotificationChannel.Push => true,
                    _ => false,
                };

                cells.Add(new NotificationMatrixCellDto(
                    eventType.Key,
                    eventType.DisplayName,
                    eventType.Description,
                    channel.ToString(),
                    isEnabled,
                    IsDefault: channel != NotificationChannel.InApp && !hasRule,
                    IsChannelImplemented: channel is NotificationChannel.InApp or NotificationChannel.Email
                        || implementedQueuedChannels.Contains(channel)));
            }
        }

        return Result<List<NotificationMatrixCellDto>>.Success(cells, nameof(GetTenantNotificationMatrixQuery));
    }
}
