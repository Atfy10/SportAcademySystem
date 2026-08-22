using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.FinanceCommands.VoidPayment;

public record VoidPaymentCommand(string PaymentNumber) : IRequest<Result<bool>>;
