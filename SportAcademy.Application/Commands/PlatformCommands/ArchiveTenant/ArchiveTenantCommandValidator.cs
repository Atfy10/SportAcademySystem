using FluentValidation;

namespace SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant
{
    public class ArchiveTenantCommandValidator : AbstractValidator<ArchiveTenantCommand>
    {
        public ArchiveTenantCommandValidator()
        {
            // Archiving is always punitive/final (never a "no big deal" transition the way
            // reactivating is), so unlike ChangeTenantStatusCommand's conditional rule, a reason
            // is required unconditionally here.
            RuleFor(x => x.Reason)
                .NotEmpty()
                .MinimumLength(5)
                .WithMessage("A reason is required when archiving a tenant.");
        }
    }
}
