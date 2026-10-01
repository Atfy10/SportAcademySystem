using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.EventTicketQueries.CheckEventTicket
{
    public class CheckEventTicketQueryHandler : IRequestHandler<CheckEventTicketQuery, Result<EventTicketCheckDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly EventTicketCheckService _check;

        public CheckEventTicketQueryHandler(EventTicketCheckService check)
        {
            _check = check;
        }

        public async Task<Result<EventTicketCheckDto>> Handle(CheckEventTicketQuery request, CancellationToken ct)
        {
            var ticket = await _check.FindAsync(request.Code, request.EventId, request.Number, ct);
            var dto = ticket is null
                ? new EventTicketCheckDto(EventTicketCheckResult.Invalid)
                : await _check.DescribeAsync(ticket, EventTicketCheckService.Judge(ticket, DateTime.UtcNow), ct);

            return Result<EventTicketCheckDto>.Success(dto, _operation);
        }
    }
}
