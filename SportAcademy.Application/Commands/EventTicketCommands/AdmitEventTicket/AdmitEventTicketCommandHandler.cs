using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;

namespace SportAcademy.Application.Commands.EventTicketCommands.AdmitEventTicket
{
    public class AdmitEventTicketCommandHandler : IRequestHandler<AdmitEventTicketCommand, Result<EventTicketCheckDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly EventTicketCheckService _check;
        private readonly IEventTicketStore _store;
        private readonly IUserContextService _userContext;

        public AdmitEventTicketCommandHandler(
            EventTicketCheckService check, IEventTicketStore store, IUserContextService userContext)
        {
            _check = check;
            _store = store;
            _userContext = userContext;
        }

        public async Task<Result<EventTicketCheckDto>> Handle(AdmitEventTicketCommand request, CancellationToken ct)
        {
            if (_userContext.UserId is not { } userId || userId == Guid.Empty)
                throw EventRuleException.NoActingUser();

            var ticket = await _check.FindAsync(request.Code, request.EventId, request.Number, ct);
            if (ticket is null)
                return Success(new EventTicketCheckDto(EventTicketCheckResult.Invalid));

            var now = DateTime.UtcNow;
            var verdict = EventTicketCheckService.Judge(ticket, now);
            if (verdict != EventTicketCheckResult.Valid)
                return Success(await _check.DescribeAsync(ticket, verdict, ct));

            if (await _store.TryAdmitAsync(ticket.Id, userId, now, ct))
            {
                ticket.AdmittedAt = now;
                ticket.AdmittedByUserId = userId;
                return Success(await _check.DescribeAsync(ticket, EventTicketCheckResult.Admitted, ct));
            }

            // Another doorman got to it first: read it back so the screen shows who and when.
            var current = await _store.FindByTokenAsync(ticket.Token, ct) ?? ticket;
            return Success(await _check.DescribeAsync(current, EventTicketCheckResult.AlreadyUsed, ct));
        }

        private Result<EventTicketCheckDto> Success(EventTicketCheckDto dto) => Result<EventTicketCheckDto>.Success(dto, _operation);
    }
}
