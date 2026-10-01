using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventTicketCommands.RevokeEventTicket
{
    // Deletes an unused ticket: its code stops working at once and its place is free again.
    public record RevokeEventTicketCommand(int Id) : IRequest<Result<bool>>, IRequiresFeature
    {
        public string FeatureKey => "event-management";
    }
}
