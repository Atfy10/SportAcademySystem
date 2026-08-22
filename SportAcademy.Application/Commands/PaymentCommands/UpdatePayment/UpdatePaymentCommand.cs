using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PaymentCommands.UpdatePayment
{
    // Corrects how an already-recorded payment was entered (e.g. wrong method, or backdating
    // the paid date) - amount/allocations are immutable once recorded and go through
    // IFinanceLedgerService.RefundPaymentAsync/VoidPaymentAsync instead.
    public record UpdatePaymentCommand(
        string PaymentNumber,
        PaymentMethod Method,
        DateTime PaidDate
    ) : IRequest<Result>;
}
