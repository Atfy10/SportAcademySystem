using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.DeleteSalaryPayment
{
    public record DeleteSalaryPaymentCommand(int Id) : IRequest<Result<bool>>;
}
