using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.MarkSalaryPaymentPaid
{
    public record MarkSalaryPaymentPaidCommand(int Id) : IRequest<Result<SalaryPaymentDto>>;
}
