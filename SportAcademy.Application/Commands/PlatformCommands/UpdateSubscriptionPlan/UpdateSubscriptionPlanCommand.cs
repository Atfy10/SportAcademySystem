using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdateSubscriptionPlan;

// SuperAdmin editing a plan's public commercial face: name, description, price, and whether/
// how it shows up on the public pricing page. Deliberately separate from
// UpdatePlanFeaturesCommand, which edits what the plan grants, not how it's sold.
public record UpdateSubscriptionPlanCommand(
    int PlanId,
    string Name,
    string? Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    bool IsPubliclyListed,
    int DisplayOrder,
    bool IsHighlighted
) : IRequest<Result<SubscriptionPlanSummaryDto>>, IAuditableCommand
{
    public string AuditEventType => "platform.plan_updated";

    // Same reasoning as UpdatePlanFeaturesCommand: a plan-wide edit has no single tenant target.
    Guid? IAuditableCommand.AuditTenantId => null;

    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
