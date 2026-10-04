using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Exceptions.PaymentExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventCommands.CancelEvent
{
    public class CancelEventCommandHandler : IRequestHandler<CancelEventCommand, Result<EventDetailsDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly IUserContextService _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EventDetailsLoader _detailsLoader;

        public CancelEventCommandHandler(
            IEventRepository eventRepository,
            IPaymentRepository paymentRepository,
            IFinanceLedgerService financeLedgerService,
            IUserContextService userContext,
            IUnitOfWork unitOfWork,
            EventDetailsLoader detailsLoader)
        {
            _eventRepository = eventRepository;
            _paymentRepository = paymentRepository;
            _financeLedgerService = financeLedgerService;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
            _detailsLoader = detailsLoader;
        }

        public async Task<Result<EventDetailsDto>> Handle(CancelEventCommand request, CancellationToken cancellationToken)
        {
            if (_userContext.UserId is not { } userId || userId == Guid.Empty)
                throw EventRuleException.NoActingUser();

            var ev = await _eventRepository.GetWithDetailsAsync(request.Id, forUpdate: true, cancellationToken)
                ?? throw new EventNotFoundException(request.Id.ToString());

            if (ev.IsCancelled)
                throw EventRuleException.AlreadyCancelled();

            if (EventStatusRules.IsCompleted(ev.IsCancelled, ev.EndsAt, TenantCalendar.Today))
                throw EventRuleException.CompletedReadOnly();

            var reason = request.Reason.Trim();

            await _unitOfWork.InTransactionAsync(async () =>
            {
                if (ev.Invoice is { } invoice)
                {
                    if (request.Mode == EventCancellationMode.RefundPayments && invoice.AmountPaid > 0)
                        await RefundEventPaymentsAsync(invoice, reason, userId, cancellationToken);

                    // Nothing (left) collected -> the bill is cancelled; money kept -> the bill
                    // closes at what was collected and the rest is waived.
                    await _financeLedgerService.CloseInvoiceForCancelledEventAsync(invoice, cancellationToken);
                }

                ev.IsCancelled = true;
                ev.CancelledAt = DateTime.UtcNow;
                ev.CancelledByUserId = userId;
                ev.CancelReason = reason;
                await _eventRepository.UpdateAsync(ev, cancellationToken);

                return true;
            }, cancellationToken);

            var dto = await _detailsLoader.LoadAsync(ev.Id, cancellationToken);
            return Result<EventDetailsDto>.Success(dto, _operation);
        }

        // Gives back, payment by payment, what each one still holds on this invoice - through the
        // ledger's normal refund, so the refund history and the revenue report record it in the
        // month it happened. A payment that also settled other invoices is refused: refunding it
        // here could pull money back off those invoices instead (the ledger reverses allocations
        // oldest-first), so that one has to be refunded by hand from the Payments page.
        private async Task RefundEventPaymentsAsync(
            Domain.Entities.Finance.Invoice invoice, string reason, Guid userId, CancellationToken ct)
        {
            var held = invoice.Allocations
                .GroupBy(a => a.PaymentNumber)
                .Select(g => (PaymentNumber: g.Key, Amount: g.Sum(a => a.Amount - a.ReversedAmount)))
                .Where(x => x.Amount > 0)
                .ToList();

            foreach (var (paymentNumber, amount) in held)
            {
                var payment = await _paymentRepository.GetWithAllocationsAsync(paymentNumber, ct)
                    ?? throw new PaymentNotFoundException(paymentNumber);

                if (payment.Allocations.Any(a => a.InvoiceId != invoice.Id && a.Amount - a.ReversedAmount > 0))
                    throw EventRuleException.PaymentSharedWithOtherInvoices(paymentNumber);

                var refundable = Math.Min(amount, payment.Amount - payment.RefundedAmount);
                if (refundable <= 0) continue;

                await _financeLedgerService.RefundPaymentAsync(paymentNumber, refundable, reason, userId, null, ct);
            }
        }
    }
}
