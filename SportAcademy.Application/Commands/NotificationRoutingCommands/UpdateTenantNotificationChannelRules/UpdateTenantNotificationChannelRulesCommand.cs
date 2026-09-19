using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;

namespace SportAcademy.Application.Commands.NotificationRoutingCommands.UpdateTenantNotificationChannelRules;

// Deliberately no IRequiresFeature: this command edits rows for every queued channel at once
// (Email and Push in the same request), so gating the whole thing on one channel's feature key
// (as this used to, "notifications-email") would block an Owner from touching their Push rows on
// a plan that only grants Push - and vice versa. The real plan/Feature boundary is enforced where
// it actually matters, at send time, in NotificationChannelDispatcher - a routing-matrix row is a
// within-plan customization, never a way to unlock a channel the plan doesn't grant.
public record UpdateTenantNotificationChannelRulesCommand(
    List<NotificationRuleUpdateDto> Rules
) : IRequest<Result<bool>>;
