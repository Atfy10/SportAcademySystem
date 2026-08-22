using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.RecordPayment;

public record RecordPaymentAllocationRequest(int InvoiceId, decimal Amount);

public record RecordPaymentCommand(
    decimal Amount,
    PaymentMethod Method,
    int BranchId,
    string? Currency,
    string? Reference,
    string? Notes,
    List<RecordPaymentAllocationRequest> Allocations
) : IRequest<Result<string>>;
