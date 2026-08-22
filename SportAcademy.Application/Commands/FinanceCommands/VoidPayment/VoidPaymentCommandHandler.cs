using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.VoidPayment;

public class VoidPaymentCommandHandler : IRequestHandler<VoidPaymentCommand, Result<bool>>
{
    private readonly IFinanceLedgerService _financeLedgerService;
    private readonly string _operation = OperationType.Update.ToString();

    public VoidPaymentCommandHandler(IFinanceLedgerService financeLedgerService)
    {
        _financeLedgerService = financeLedgerService;
    }

    public async Task<Result<bool>> Handle(VoidPaymentCommand request, CancellationToken ct)
    {
        await _financeLedgerService.VoidPaymentAsync(request.PaymentNumber, ct);
        return Result<bool>.Success(true, _operation);
    }
}
