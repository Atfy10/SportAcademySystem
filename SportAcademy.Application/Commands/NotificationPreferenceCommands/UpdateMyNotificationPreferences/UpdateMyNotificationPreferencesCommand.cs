using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Commands.NotificationPreferenceCommands.UpdateMyNotificationPreferences;

// Deliberately no IRequiresFeature - same reasoning as UpdateTenantNotificationChannelRulesCommand:
// this covers every channel's opt-out in one request, so it can't be gated on any single
// channel's Feature key. A user narrowing their own Email/Push/WhatsApp preference is harmless
// regardless of what the tenant's plan grants; NotificationChannelDispatcher is what actually
// enforces the plan boundary before anything sends.
public record UpdateMyNotificationPreferencesCommand(
    List<NotificationPreferenceUpdateDto> Preferences
) : IRequest<Result<bool>>;
