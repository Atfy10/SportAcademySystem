using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.FinanceCommands.RefundPayment;

public record RefundPaymentCommand(string PaymentNumber, decimal Amount) : IRequest<Result<bool>>;
