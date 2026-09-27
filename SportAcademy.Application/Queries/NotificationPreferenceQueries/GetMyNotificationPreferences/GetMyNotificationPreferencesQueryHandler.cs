using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.NotificationPreferenceQueries.GetMyNotificationPreferences;

public class GetMyNotificationPreferencesQueryHandler
    : IRequestHandler<GetMyNotificationPreferencesQuery, Result<List<NotificationPreferenceDto>>>
{
    // InApp is deliberately excluded here - it's always on and never user-configurable, so it
    // has no row in the DTO for MyProfile's card to even render as a toggle.
    private static readonly NotificationChannel[] ConfigurableChannels =
        [NotificationChannel.Email, NotificationChannel.Push, NotificationChannel.WhatsApp];

    private readonly INotificationSettingsRepository _repository;
    private readonly IUserContextService _userContext;

    public GetMyNotificationPreferencesQueryHandler(
        INotificationSettingsRepository repository, IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<List<NotificationPreferenceDto>>> Handle(
        GetMyNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId
            ?? throw new InvalidOperationException("GetMyNotificationPreferencesQuery invoked without an authenticated user.");

        var prefs = await _repository.GetUserPreferencesAsync(userId, cancellationToken);
        var implementedQueuedChannels = await _repository.GetImplementedQueuedChannelsAsync(cancellationToken);

        var result = ConfigurableChannels
            .Select(channel => new NotificationPreferenceDto(
                channel.ToString(),
                // Absence of a row means "not opted out" - the tenant's matrix already decides
                // whether this channel fires at all for a given event; this is purely a narrowing
                // opt-out on top of that, so "no row" must default to enabled, not disabled.
                prefs.TryGetValue(channel, out var isEnabled) ? isEnabled : true,
                channel == NotificationChannel.Email || implementedQueuedChannels.Contains(channel)))
            .ToList();

        return Result<List<NotificationPreferenceDto>>.Success(result, nameof(GetMyNotificationPreferencesQuery));
    }
}
