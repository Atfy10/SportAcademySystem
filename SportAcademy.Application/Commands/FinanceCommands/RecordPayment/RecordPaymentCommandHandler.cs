using MediatR;
using SportAcademy.Application.Common;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Add.ToString();

    public RecordPaymentCommandHandler(
        IFinanceLedgerService financeLedgerService,
        IUserContextService userContext,
        IUserRepository userRepository,
        IPublisher publisher,
        IUnitOfWork unitOfWork)
    {
        _financeLedgerService = financeLedgerService;
        _userContext = userContext;
        _userRepository = userRepository;
        _publisher = publisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<string>> Handle(RecordPaymentCommand request, CancellationToken ct)
    {
        // A blank currency lets the ledger take it from the invoices being paid - an invoice billed
        // before the tenant changed its default currency must still be settled in its own one.
        var payment = await _unitOfWork.InTransactionAsync(() => _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
            Amount: request.Amount,
            PaymentTypeId: request.PaymentTypeId,
            BranchId: request.BranchId,
            Currency: string.IsNullOrWhiteSpace(request.Currency) ? null : request.Currency,
            Reference: request.Reference,
            Notes: request.Notes,
            RecordedByUserId: _userContext.UserId,
            Allocations: request.Allocations
                .Select(a => new PaymentAllocationInput(a.InvoiceId, a.Amount))
                .ToList()
        ), ct), ct);

        // Resolved here, not in the event handler - IUserContextService is only reliably valid
        // for the duration of this request; the resolved name travels with the event instead of
        // being re-derived from ambient context later.
        var actorName = _userContext.UserId is { } userId
            ? await _userRepository.GetDisplayNameAsync(userId, ct)
            : "System";
        await _publisher.Publish(new PaymentRecordedEvent(payment.PaymentNumber, request.Amount, payment.Currency, actorName), ct);

        return Result<string>.Success(payment.PaymentNumber, _operation);
    }
}
