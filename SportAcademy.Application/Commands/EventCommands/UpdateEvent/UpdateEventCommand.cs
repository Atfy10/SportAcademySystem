using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventCommands.UpdateEvent
{
    // Full replacement of the booking's details (the edit form always sends every field).
    // Payments are not taken here - collect through the Payments page against the invoice.
    // BalanceDueDate, when set, moves the collect date of whatever is still owed.
    public record UpdateEventCommand(
        int Id,
        string Title,
        int BranchId,
        int EventCustomerId,
        bool WithDecorations,
        decimal Price,
        decimal DecorationFee,
        int Capacity,
        DateTime StartsAt,
        DateTime EndsAt,
        string? Notes,
        DateOnly? BalanceDueDate
    ) : IRequest<Result<EventDetailsDto>>, IRequiresFeature, IBranchScopedRequest, IRequiresActiveBranch
    {
        public string FeatureKey => "event-management";
    }
}
