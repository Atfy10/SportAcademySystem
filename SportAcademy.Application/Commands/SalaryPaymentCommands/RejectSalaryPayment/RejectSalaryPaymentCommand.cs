using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.FinanceDtos;

namespace SportAcademy.Application.Commands.SalaryPaymentCommands.RejectSalaryPayment
{
    public record RejectSalaryPaymentCommand(int Id, string RejectionReason) : IRequest<Result<SalaryPaymentDto>>;
}
