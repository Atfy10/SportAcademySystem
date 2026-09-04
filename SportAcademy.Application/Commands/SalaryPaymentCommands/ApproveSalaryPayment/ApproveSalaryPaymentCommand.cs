using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.ApproveSalaryPayment
{
    public record ApproveSalaryPaymentCommand(int Id) : IRequest<Result<SalaryPaymentDto>>;
}
