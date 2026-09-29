using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.FinanceCommands.VoidPayment;

public record VoidPaymentCommand(string PaymentNumber, string Reason, DateOnly? NewDueDate) : IRequest<Result<bool>>;
