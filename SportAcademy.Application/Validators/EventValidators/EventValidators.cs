using FluentValidation;
using SportAcademy.Application.Commands.EventCommands.CancelEvent;
using SportAcademy.Application.Commands.EventCommands.CreateEvent;
using SportAcademy.Application.Commands.EventCommands.UpdateEvent;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Validators.EventValidators
{
    public class CreateEventValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventValidator(
            IRegionalValidationService regionalValidation, ITenantSettingsCountryReader countryReader)
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Event title is required.")
                .MaximumLength(200).WithMessage("Event title must not exceed 200 characters.");

            RuleFor(x => x.BranchId).ApplyIdRuleFor("Branch");

            RuleFor(x => x)
                .Must(x => (x.EventCustomerId is not null) ^ (x.NewCustomer is not null))
                .WithName("Customer")
                .WithMessage("Choose an existing customer or enter a new one.");

            RuleFor(x => x.EventCustomerId).ApplyOptionalIdRuleFor("Customer");

            When(x => x.NewCustomer is not null, () =>
            {
                RuleFor(x => x.NewCustomer!.FullName)
                    .NotEmpty().WithMessage("Customer name is required.")
                    .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters.");
                RuleFor(x => x.NewCustomer!.PhoneNumber)
                    .NotEmpty().WithMessage("Customer phone number is required.")
                    .ApplyPhoneRuleFor(regionalValidation, countryReader);
                RuleFor(x => x.NewCustomer!.NationalityCategoryId).ApplyIdRuleFor("Nationality category");
            });

            EventRules.Apply(this, x => x.Price, x => x.DecorationFee,
                x => x.Capacity, x => x.StartsAt, x => x.EndsAt, x => x.Notes);

            RuleFor(x => x.AmountPaidNow)
                .GreaterThanOrEqualTo(0).WithMessage("The amount collected now can't be negative.");

            RuleFor(x => x.PaymentTypeId)
                .NotNull().WithMessage("Choose how the payment was made.")
                .When(x => x.AmountPaidNow > 0);
            RuleFor(x => x.PaymentTypeId).ApplyOptionalIdRuleFor("Payment type");

            RuleFor(x => x.BalanceDueDate)
                .Must(d => d is null || d.Value >= TenantCalendar.Today)
                .WithMessage("The balance due date can't be in the past.");

            RuleFor(x => x.PaymentNote).MaximumLength(1000);
        }
    }

    public class UpdateEventValidator : AbstractValidator<UpdateEventCommand>
    {
        public UpdateEventValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Event");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Event title is required.")
                .MaximumLength(200).WithMessage("Event title must not exceed 200 characters.");

            RuleFor(x => x.BranchId).ApplyIdRuleFor("Branch");
            RuleFor(x => x.EventCustomerId).ApplyIdRuleFor("Customer");

            EventRules.Apply(this, x => x.Price, x => x.DecorationFee,
                x => x.Capacity, x => x.StartsAt, x => x.EndsAt, x => x.Notes);

            RuleFor(x => x.BalanceDueDate)
                .Must(d => d is null || d.Value >= TenantCalendar.Today)
                .WithMessage("The balance due date can't be in the past.");
        }
    }

    public class CancelEventValidator : AbstractValidator<CancelEventCommand>
    {
        public CancelEventValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Event");
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Please give a reason for cancelling.")
                .MaximumLength(500).WithMessage("The reason must not exceed 500 characters.");
            RuleFor(x => x.Mode).IsInEnum();
        }
    }

    // The booking rules shared by create and update. A decoration fee sent without decorations
    // is ignored (the handlers store 0), not rejected.
    internal static class EventRules
    {
        public static void Apply<T>(
            AbstractValidator<T> v,
            System.Linq.Expressions.Expression<Func<T, decimal>> price,
            System.Linq.Expressions.Expression<Func<T, decimal>> decorationFee,
            System.Linq.Expressions.Expression<Func<T, int>> capacity,
            System.Linq.Expressions.Expression<Func<T, DateTime>> startsAt,
            System.Linq.Expressions.Expression<Func<T, DateTime>> endsAt,
            System.Linq.Expressions.Expression<Func<T, string?>> notes)
        {
            v.RuleFor(price).GreaterThanOrEqualTo(0).WithMessage("Price can't be negative.");

            v.RuleFor(decorationFee)
                .GreaterThanOrEqualTo(0).WithMessage("Decoration fee can't be negative.");

            v.RuleFor(capacity)
                .GreaterThan(0).WithMessage("Capacity must be at least 1.")
                .LessThanOrEqualTo(100000).WithMessage("Capacity is too large.");

            var start = startsAt.Compile();
            v.RuleFor(endsAt)
                .Must((x, end) => end > start(x))
                .WithMessage("The event must end after it starts.");

            v.RuleFor(notes).MaximumLength(1000);
        }
    }
}
