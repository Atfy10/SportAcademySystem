using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.NotificationPreferenceCommands.UpdateMyNotificationPreferences;

public class UpdateMyNotificationPreferencesCommandHandler
    : IRequestHandler<UpdateMyNotificationPreferencesCommand, Result<bool>>
{
    private readonly INotificationSettingsRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateMyNotificationPreferencesCommandHandler(
        INotificationSettingsRepository repository, IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(UpdateMyNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        // Silently ignored, not rejected outright - InApp simply isn't a channel this DTO can
        // express (GetMyNotificationPreferencesQuery never returns it), so a client sending it
        // anyway is dropped rather than failing the whole save over one unrecognized entry.
        var resolved = request.Preferences
            .Where(p => Enum.TryParse<NotificationChannel>(p.Channel, true, out var channel) && channel != NotificationChannel.InApp)
            .Select(p =>
            {
                Enum.TryParse<NotificationChannel>(p.Channel, true, out var channel);
                return (Channel: channel, p.IsEnabled);
            })
            .ToList();

        var userId = _userContext.UserId
            ?? throw new InvalidOperationException("UpdateMyNotificationPreferencesCommand invoked without an authenticated user.");

        await _repository.UpsertUserPreferencesAsync(userId, resolved, cancellationToken);

        return Result<bool>.Success(true, _operation);
    }
}
