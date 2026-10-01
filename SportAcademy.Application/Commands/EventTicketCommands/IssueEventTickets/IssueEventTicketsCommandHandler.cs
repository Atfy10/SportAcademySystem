using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventTicketCommands.IssueEventTickets
{
    public class IssueEventTicketsCommandHandler : IRequestHandler<IssueEventTicketsCommand, Result<List<EventTicketDto>>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly IEventTicketStore _store;

        public IssueEventTicketsCommandHandler(IEventRepository eventRepository, IEventTicketStore store)
        {
            _eventRepository = eventRepository;
            _store = store;
        }

        public async Task<Result<List<EventTicketDto>>> Handle(IssueEventTicketsCommand request, CancellationToken ct)
        {
            var ev = await _eventRepository.GetByIdAsync(request.EventId, ct)
                ?? throw new EventNotFoundException(request.EventId.ToString());

            var now = DateTime.UtcNow;
            if (EventEntryRules.TicketsTerminated(ev.IsCancelled, ev.EndsAt, now))
                throw EventRuleException.TicketsTerminated();

            var taken = (await _store.GetNumbersAsync(ev.Id, ct)).ToHashSet();
            var remaining = Math.Max(0, ev.Capacity - taken.Count);
            if (request.Count > remaining)
                throw EventRuleException.TooManyTickets(remaining);

            // Random numbers (EventEntryRules.NewTicketNumber), never one this event already uses.
            var guestName = request.Count == 1 ? EventTicketMapper.CleanName(request.GuestName) : null;
            var tickets = Enumerable.Range(0, request.Count)
                .Select(_ => new EventTicket
                {
                    EventId = ev.Id,
                    Number = EventEntryRules.NewTicketNumber(taken),
                    Token = EventEntryRules.NewToken(),
                    GuestName = guestName,
                    IssuedAt = now,
                })
                .ToList();

            // Saved together with a touch of the event, so its rowversion guards the capacity check
            // above: if another issue (or a capacity edit) lands first, this save fails with a
            // concurrency conflict (409, "try again") instead of going over capacity.
            ev.UpdatedAt = now;
            await _store.AddRangeAsync(tickets, ct);

            var noNames = new Dictionary<Guid, string>();
            return Result<List<EventTicketDto>>.Success(
                tickets.Select(t => EventTicketMapper.ToDto(t, noNames)).ToList(), _operation);
        }
    }
}
