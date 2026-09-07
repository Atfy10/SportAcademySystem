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
        }
    }
}
