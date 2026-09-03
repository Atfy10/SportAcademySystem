using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.FinanceCommands.RecordPayment;

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<string>>
{
    private readonly IFinanceLedgerService _financeLedgerService;
    private readonly IUserContextService _userContext;
    private readonly IUserRepository _userRepository;
    private readonly IPublisher _publisher;
    private readonly string _operation = OperationType.Add.ToString();

    public RecordPaymentCommandHandler(
        IFinanceLedgerService financeLedgerService,
        IUserContextService userContext,
        IUserRepository userRepository,
        IPublisher publisher)
    {
        _financeLedgerService = financeLedgerService;
        _userContext = userContext;
        _userRepository = userRepository;
        _publisher = publisher;
    }

    public async Task<Result<string>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "KWD" : request.Currency;

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
