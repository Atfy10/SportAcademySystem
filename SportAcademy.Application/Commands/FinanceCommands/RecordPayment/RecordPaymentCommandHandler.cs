using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.RecordPayment;

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<string>>
{
    private readonly IFinanceLedgerService _financeLedgerService;
    private readonly IUserContextService _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;
    private readonly ITenantSettingsCurrencyReader _currencyReader;
    private readonly string _operation = OperationType.Add.ToString();

    public RecordPaymentCommandHandler(
        IFinanceLedgerService financeLedgerService,
        IUserContextService userContext,
        IUserRepository userRepository,
        IPublisher publisher,
        ITenantSettingsCurrencyReader currencyReader)
    {
        _financeLedgerService = financeLedgerService;
        _userContext = userContext;
        _userRepository = userRepository;
        _publisher = publisher;
        _currencyReader = currencyReader;
    }

    public async Task<Result<string>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        // Falls back to the tenant's configured currency (not a hardcoded literal) only when
        // the caller didn't specify one explicitly - a payment recorded in a specific currency
        // should keep it even if the tenant's default changes later.
        var currency = string.IsNullOrWhiteSpace(request.Currency)
            ? await _currencyReader.GetCurrencyAsync(ct) ?? "KWD"
            : request.Currency;

        var payment = await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
            Amount: request.Amount,
            PaymentTypeId: request.PaymentTypeId,
            BranchId: request.BranchId,
            Currency: currency,
            Reference: request.Reference,
            Notes: request.Notes,
            RecordedByUserId: _userContext.UserId,
            Allocations: request.Allocations
                .Select(a => new PaymentAllocationInput(a.InvoiceId, a.Amount))
                .ToList()
        ), ct);

        // Resolved here, not in the event handler - IUserContextService is only reliably valid
        // for the duration of this request; the resolved name travels with the event instead of
        // being re-derived from ambient context later.
        var actorName = _userContext.UserId is { } userId
            ? await _userRepository.GetDisplayNameAsync(userId, ct)
            : "System";
        await _publisher.Publish(new PaymentRecordedEvent(payment.PaymentNumber, request.Amount, currency, actorName), ct);

        return Result<string>.Success(payment.PaymentNumber, _operation);
    }
}
