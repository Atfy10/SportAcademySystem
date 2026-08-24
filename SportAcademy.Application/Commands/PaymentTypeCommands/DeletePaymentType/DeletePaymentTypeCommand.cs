using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.PaymentTypeCommands.DeletePaymentType
{
    public record DeletePaymentTypeCommand(int Id) : IRequest<Result<bool>>;
}
