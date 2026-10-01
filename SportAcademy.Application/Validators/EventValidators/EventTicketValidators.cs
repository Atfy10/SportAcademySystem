using FluentValidation;
using SportAcademy.Application.Commands.EventTicketCommands.AdmitEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.IssueEventTickets;
using SportAcademy.Application.Commands.EventTicketCommands.UpdateEventTicket;
using SportAcademy.Application.Queries.EventTicketQueries.CheckEventTicket;
using SportAcademy.Application.Queries.EventTicketQueries.GetPublicEventTicket;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Validators.EventValidators
{
    public class IssueEventTicketsValidator : AbstractValidator<IssueEventTicketsCommand>
    {
        public IssueEventTicketsValidator()
        {
            RuleFor(x => x.EventId).ApplyIdRuleFor("Event");

            RuleFor(x => x.Count)
                .GreaterThan(0).WithMessage("Issue at least one ticket.")
                .LessThanOrEqualTo(EventEntryRules.MaxTicketsPerIssue)
                .WithMessage($"Issue at most {EventEntryRules.MaxTicketsPerIssue} tickets at a time.");

            RuleFor(x => x.GuestName)
                .MaximumLength(100).WithMessage("Guest name must not exceed 100 characters.");

            RuleFor(x => x.GuestName)
                .Must(name => string.IsNullOrWhiteSpace(name))
                .When(x => x.Count != 1)
                .WithMessage("A guest name can only be given when issuing a single ticket.");
        }
    }

    public class UpdateEventTicketValidator : AbstractValidator<UpdateEventTicketCommand>
    {
        public UpdateEventTicketValidator()
        {
            RuleFor(x => x.Id).ApplyIdRuleFor("Ticket");
            RuleFor(x => x.GuestName)
                .MaximumLength(100).WithMessage("Guest name must not exceed 100 characters.");
        }
    }

    // Either the scanned code, or the event + ticket number typed in by hand.
    internal static class TicketReferenceRules
    {
        public static bool IsComplete(string? code, int? eventId, int? number)
            => !string.IsNullOrWhiteSpace(code) || (eventId > 0 && number > 0);
    }

    public class CheckEventTicketValidator : AbstractValidator<CheckEventTicketQuery>
    {
        public CheckEventTicketValidator()
        {
            RuleFor(x => x.Code).MaximumLength(512);
            RuleFor(x => x)
                .Must(x => TicketReferenceRules.IsComplete(x.Code, x.EventId, x.Number))
                .WithName("Ticket")
                .WithMessage("Scan a ticket, or choose the event and enter the ticket number.");
        }
    }

    public class AdmitEventTicketValidator : AbstractValidator<AdmitEventTicketCommand>
    {
        public AdmitEventTicketValidator()
        {
            RuleFor(x => x.Code).MaximumLength(512);
            RuleFor(x => x)
                .Must(x => TicketReferenceRules.IsComplete(x.Code, x.EventId, x.Number))
                .WithName("Ticket")
                .WithMessage("Scan a ticket, or choose the event and enter the ticket number.");
        }
    }

    public class GetPublicEventTicketValidator : AbstractValidator<GetPublicEventTicketQuery>
    {
        public GetPublicEventTicketValidator()
        {
            RuleFor(x => x.Token).NotEmpty().MaximumLength(64);
        }
    }
}
