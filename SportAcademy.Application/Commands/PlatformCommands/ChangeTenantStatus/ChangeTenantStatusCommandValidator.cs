using FluentValidation;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus
{
    public class ChangeTenantStatusCommandValidator : AbstractValidator<ChangeTenantStatusCommand>
    {
        public ChangeTenantStatusCommandValidator()
        {
            // Only required when the transition is punitive (moving the tenant away from a
            // working state) - reactivating (-> Active) needs no justification, there's nothing
            // to explain.
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MinimumLength(5)
                .When(x => x.NewStatus is TenantStatus.Suspended or TenantStatus.Archived or TenantStatus.Inactive)
                .WithMessage("A reason is required when suspending, deactivating, or archiving a tenant.");

            // PendingLimitSelection is never a manual SuperAdmin choice through this generic
            // command - it needs a fresh reconciliation snapshot and deadline that only
            // LimitReconciliationService (the automatic trigger) or ReopenLimitReconciliationCommand
            // (the Suspended -> PendingLimitSelection "give them another window" case) create.
            // Without this guard, TenantStatusPolicy allowing (Active, PendingLimitSelection) at
            // all would let this command set a tenant to that status with no
            // TenantLimitReconciliation row behind it - frozen in "you must select" with nothing
            // to select and no deadline.
            RuleFor(x => x.NewStatus)
                .NotEqual(TenantStatus.PendingLimitSelection)
                .WithMessage("Use the reopen-reconciliation action instead of setting this status directly.");
        }
    }
}
