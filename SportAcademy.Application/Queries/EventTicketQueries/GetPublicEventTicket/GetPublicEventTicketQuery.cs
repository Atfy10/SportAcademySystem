using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;

namespace SportAcademy.Application.Queries.EventTicketQueries.GetPublicEventTicket
{
    // The guest opened their ticket link (/ticket/{token}). Anonymous and read-only: it shows the
    // ticket so the guest can present it at the door - it never lets anyone in.
    public record GetPublicEventTicketQuery(string Token) : IRequest<Result<PublicEventTicketDto>>;
}
