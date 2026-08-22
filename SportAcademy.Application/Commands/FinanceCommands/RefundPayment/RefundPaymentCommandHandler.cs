using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.RefundPayment;

public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result<bool>>
{
    private readonly IFinanceLedgerService _financeLedgerService;
    private readonly string _operation = OperationType.Update.ToString();

    public RefundPaymentCommandHandler(IFinanceLedgerService financeLedgerService)
    {
        _financeLedgerService = financeLedgerService;
    }

    public async Task<Result<bool>> Handle(RefundPaymentCommand request, CancellationToken ct)
    {
        await _financeLedgerService.RefundPaymentAsync(request.PaymentNumber, request.Amount, ct);
        return Result<bool>.Success(true, _operation);
    }
}
