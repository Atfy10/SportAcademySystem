using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.FinanceCommands.RefundPayment;

// The refunded money is owed again on the invoice it came from. NewDueDate is when that reopened
// balance should be collected; null keeps the invoice's existing due date.
public record RefundPaymentCommand(string PaymentNumber, decimal Amount, string Reason, DateOnly? NewDueDate)
    : IRequest<Result<bool>>;
