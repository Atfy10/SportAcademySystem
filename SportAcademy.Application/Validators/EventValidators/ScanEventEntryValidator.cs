using FluentValidation;
using SportAcademy.Application.Commands.EventCommands.ScanEventEntry;

namespace SportAcademy.Application.Validators.EventValidators
{
    public class ScanEventEntryValidator : AbstractValidator<ScanEventEntryCommand>
    {
        public ScanEventEntryValidator()
        {
            RuleFor(x => x.Token).NotEmpty().MaximumLength(64);
            // The entry page's own random id: letters, digits and dashes only.
            RuleFor(x => x.DeviceKey)
                .NotEmpty()
                .Length(8, 64)
                .Matches("^[A-Za-z0-9-]+$");
        }
    }
}
