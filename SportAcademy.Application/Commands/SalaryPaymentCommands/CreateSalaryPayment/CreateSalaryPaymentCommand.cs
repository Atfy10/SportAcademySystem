using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.CreateSalaryPayment
{
    public record CreateSalaryPaymentCommand(
        int EmployeeId,
        decimal Amount,
        DateOnly PeriodMonth,
        int? PaymentTypeId,
        string? Notes
    ) : IRequest<Result<SalaryPaymentDto>>;
}
