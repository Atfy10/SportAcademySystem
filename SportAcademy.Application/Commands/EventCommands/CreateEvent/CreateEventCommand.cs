using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.EventCommands.CreateEvent
{
    // A customer typed in on the booking form. If the phone number already belongs to a
    // customer, that customer is used as-is (never overwritten) - see CreateEventCommandHandler.
    public record NewEventCustomerInput(string FullName, string PhoneNumber, int NationalityCategoryId);

    // Exactly one of EventCustomerId / NewCustomer is set. StartsAt/EndsAt are the academy's
    // local wall-clock time. AmountPaidNow is what's collected at booking (0 = nothing yet, the
    // total = paid in full); anything left is owed by BalanceDueDate (defaults to the event day).
    public record CreateEventCommand(
        string Title,
        int BranchId,
        int? EventCustomerId,
        NewEventCustomerInput? NewCustomer,
        bool WithDecorations,
        decimal Price,
        decimal DecorationFee,
        int Capacity,
        DateTime StartsAt,
        DateTime EndsAt,
        string? Notes,
        decimal AmountPaidNow,
        int? PaymentTypeId,
        DateOnly? BalanceDueDate,
        string? PaymentNote
    ) : IRequest<Result<EventDetailsDto>>, IRequiresFeature, IBranchScopedRequest, IRequiresActiveBranch
    {
        public string FeatureKey => "event-management";
    }
}
