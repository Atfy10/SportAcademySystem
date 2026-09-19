using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.NotificationRoutingCommands.UpdateTenantNotificationChannelRules;

public class UpdateTenantNotificationChannelRulesCommandHandler
    : IRequestHandler<UpdateTenantNotificationChannelRulesCommand, Result<bool>>
{
    private readonly INotificationSettingsRepository _repository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateTenantNotificationChannelRulesCommandHandler(
        INotificationSettingsRepository repository, IUserContextService userContext)
    {
        _repository = repository;
        _userContext = userContext;
    }

    public async Task<Result<bool>> Handle(UpdateTenantNotificationChannelRulesCommand request, CancellationToken cancellationToken)
    {
        if (request.Rules.Any(r => string.Equals(r.Channel, nameof(NotificationChannel.InApp), StringComparison.OrdinalIgnoreCase)))
            return Result<bool>.Failure(_operation, "In-app notifications can't be turned off.");

        var tenantId = _userContext.TenantId
            ?? throw new InvalidOperationException("UpdateTenantNotificationChannelRulesCommand invoked without a resolved tenant context.");

        var eventTypes = (await _repository.GetAllEventTypesAsync(cancellationToken))
            .ToDictionary(e => e.Key, StringComparer.OrdinalIgnoreCase);

        var resolved = new List<(Guid EventTypeId, NotificationChannel Channel, bool IsEnabled)>();
        foreach (var rule in request.Rules)
        {
            if (!eventTypes.TryGetValue(rule.EventTypeKey, out var eventType)) continue;
            if (!Enum.TryParse<NotificationChannel>(rule.Channel, true, out var channel)) continue;

            resolved.Add((eventType.Id, channel, rule.IsEnabled));
        }

        var actorId = _userContext.UserId?.ToString();
        await _repository.UpsertTenantRulesAsync(tenantId, resolved, actorId, cancellationToken);

        return Result<bool>.Success(true, _operation);
    }
}
