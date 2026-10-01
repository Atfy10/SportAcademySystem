using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventTicketCommands.UpdateEventTicket
{
    public class UpdateEventTicketCommandHandler : IRequestHandler<UpdateEventTicketCommand, Result<EventTicketDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IEventTicketStore _store;
        private readonly IUserRepository _userRepository;

        public UpdateEventTicketCommandHandler(IEventTicketStore store, IUserRepository userRepository)
        {
            _store = store;
            _userRepository = userRepository;
        }

        public async Task<Result<EventTicketDto>> Handle(UpdateEventTicketCommand request, CancellationToken ct)
        {
            var ticket = await _store.GetForUpdateAsync(request.Id, ct)
                ?? throw new EventTicketNotFoundException(request.Id.ToString());

            if (EventEntryRules.TicketsTerminated(ticket.Event.IsCancelled, ticket.Event.EndsAt, DateTime.UtcNow))
                throw EventRuleException.TicketsTerminated();

            // A used ticket is the record of who came in - its name stays as it was.
            if (ticket.IsAdmitted)
                throw EventRuleException.TicketAlreadyUsed();

            ticket.GuestName = EventTicketMapper.CleanName(request.GuestName);
            await _store.SaveChangesAsync(ct);

            var names = await _userRepository.GetDisplayNamesAsync(EventTicketMapper.UserIds([ticket]), ct);
            return Result<EventTicketDto>.Success(EventTicketMapper.ToDto(ticket, names), _operation);
        }
    }
}
