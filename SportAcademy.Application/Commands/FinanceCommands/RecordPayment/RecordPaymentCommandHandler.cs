using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.RecordPayment;

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<string>>
{
    private readonly IFinanceLedgerService _financeLedgerService;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Add.ToString();

    public RecordPaymentCommandHandler(IFinanceLedgerService financeLedgerService, IUserContextService userContext)
    {
        _financeLedgerService = financeLedgerService;
        _userContext = userContext;
    }

    public async Task<Result<string>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var payment = await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
            Amount: request.Amount,
            Method: request.Method,
            BranchId: request.BranchId,
            Currency: string.IsNullOrWhiteSpace(request.Currency) ? "KWD" : request.Currency,
            Reference: request.Reference,
            Notes: request.Notes,
            RecordedByUserId: _userContext.UserId,
            Allocations: request.Allocations
                .Select(a => new PaymentAllocationInput(a.InvoiceId, a.Amount))
                .ToList()
        ), ct);

        return Result<string>.Success(payment.PaymentNumber, _operation);
    }
}
