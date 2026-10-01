using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventTicketCommands.ReissueEventTicket
{
    public class ReissueEventTicketCommandHandler : IRequestHandler<ReissueEventTicketCommand, Result<EventTicketDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IEventTicketStore _store;

        public ReissueEventTicketCommandHandler(IEventTicketStore store)
        {
            _store = store;
        }

        public async Task<Result<EventTicketDto>> Handle(ReissueEventTicketCommand request, CancellationToken ct)
        {
            var ticket = await _store.GetForUpdateAsync(request.Id, ct)
                ?? throw new EventTicketNotFoundException(request.Id.ToString());

            if (EventEntryRules.TicketsTerminated(ticket.Event.IsCancelled, ticket.Event.EndsAt, DateTime.UtcNow))
                throw EventRuleException.TicketsTerminated();

            if (ticket.IsAdmitted)
                throw EventRuleException.TicketAlreadyUsed();

            // A new code AND a new number: whoever holds the old ticket can't get in with its QR,
            // nor by reading its number out at the door (the manual fallback finds by number).
            var taken = (await _store.GetNumbersAsync(ticket.EventId, ct)).ToHashSet();
            ticket.Token = EventEntryRules.NewToken();
            ticket.Number = EventEntryRules.NewTicketNumber(taken);
            await _store.SaveChangesAsync(ct);

            return Result<EventTicketDto>.Success(EventTicketMapper.ToDto(ticket, new Dictionary<Guid, string>()), _operation);
        }
    }
}
