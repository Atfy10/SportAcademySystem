using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.PlatformCommands.ClonePlan;

// Deep-copies a source plan's SubscriptionPlanFeature and PlanLimit rows into a new, private
// (IsCustom) plan owned by one tenant - the "this shape will be resold/reused" path (D2 in
// PLAN_LIMITS_DESIGN.md). A one-off tweak for a single tenant is a TenantLimitOverride/locked
// TenantFeature instead and creates no plan row at all - only reach for this when the negotiated
// shape deserves its own named plan (e.g. converting a marketing-site bundle-builder lead).
public record ClonePlanCommand(int SourcePlanId, string Name, string Code, Guid OwnerTenantId)
    : IRequest<Result<SubscriptionPlanSummaryDto>>, IAuditableCommand
{
    public string AuditEventType => "platform.plan_cloned";
    Guid? IAuditableCommand.AuditTenantId => OwnerTenantId;

    public object? ResolvedBeforeState { get; set; }
    object? IAuditableCommand.AuditBeforeState => ResolvedBeforeState;
}
