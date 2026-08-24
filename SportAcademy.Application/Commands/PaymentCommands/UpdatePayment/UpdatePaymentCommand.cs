using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PaymentCommands.UpdatePayment
{
    // Corrects how an already-recorded payment was entered (e.g. wrong type, or backdating
    // the paid date) - amount/allocations are immutable once recorded and go through
    // IFinanceLedgerService.RefundPaymentAsync/VoidPaymentAsync instead.
    public record UpdatePaymentCommand(
        string PaymentNumber,
        int PaymentTypeId,
        DateTime PaidDate
    ) : IRequest<Result>;
}
