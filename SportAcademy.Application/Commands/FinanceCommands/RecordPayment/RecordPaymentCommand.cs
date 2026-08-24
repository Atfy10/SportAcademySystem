using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.FinanceCommands.RecordPayment;

public record RecordPaymentAllocationRequest(int InvoiceId, decimal Amount);

public record RecordPaymentCommand(
    decimal Amount,
    int PaymentTypeId,
    int BranchId,
    string? Currency,
    string? Reference,
    string? Notes,
    List<RecordPaymentAllocationRequest> Allocations
) : IRequest<Result<string>>;
