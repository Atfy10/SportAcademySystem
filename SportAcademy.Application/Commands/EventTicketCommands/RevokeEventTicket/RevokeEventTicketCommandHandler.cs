using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventTicketCommands.RevokeEventTicket
{
    public class RevokeEventTicketCommandHandler : IRequestHandler<RevokeEventTicketCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly IEventTicketStore _store;

        public RevokeEventTicketCommandHandler(IEventTicketStore store)
        {
            _store = store;
        }

        public async Task<Result<bool>> Handle(RevokeEventTicketCommand request, CancellationToken ct)
        {
            var ticket = await _store.GetForUpdateAsync(request.Id, ct)
                ?? throw new EventTicketNotFoundException(request.Id.ToString());

            if (EventEntryRules.TicketsTerminated(ticket.Event.IsCancelled, ticket.Event.EndsAt, DateTime.UtcNow))
                throw EventRuleException.TicketsTerminated();

            // A used ticket is the record that someone came in - it stays.
            if (ticket.IsAdmitted)
                throw EventRuleException.TicketAlreadyUsed();

            await _store.RemoveAsync(ticket, ct);
            return Result<bool>.Success(true, _operation);
        }
    }
}
