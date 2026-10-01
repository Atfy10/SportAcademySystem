using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventCommands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result<EventDetailsDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly IEventCustomerRepository _customerRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly ITenantSettingsCurrencyReader _currencyReader;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EventDetailsLoader _detailsLoader;
        private readonly IEventTicketStore _ticketStore;

        public UpdateEventCommandHandler(
            IEventRepository eventRepository,
            IEventCustomerRepository customerRepository,
            IBranchRepository branchRepository,
            IFinanceLedgerService financeLedgerService,
            ITenantSettingsCurrencyReader currencyReader,
            IUnitOfWork unitOfWork,
            EventDetailsLoader detailsLoader,
            IEventTicketStore ticketStore)
        {
            _eventRepository = eventRepository;
            _customerRepository = customerRepository;
            _branchRepository = branchRepository;
            _financeLedgerService = financeLedgerService;
            _currencyReader = currencyReader;
            _unitOfWork = unitOfWork;
            _detailsLoader = detailsLoader;
            _ticketStore = ticketStore;
        }

        public async Task<Result<EventDetailsDto>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var ev = await _eventRepository.GetWithDetailsAsync(request.Id, forUpdate: true, cancellationToken)
                ?? throw new EventNotFoundException(request.Id.ToString());

            if (ev.IsCancelled)
                throw EventRuleException.CancelledReadOnly();

            // Every issued ticket keeps its place - revoke unused ones first to go lower.
            var tickets = await _ticketStore.GetCountsAsync(ev.Id, cancellationToken);
            if (request.Capacity < tickets.Issued)
                throw EventRuleException.CapacityBelowIssued(tickets.Issued);

            // Once an event has ended its tickets are terminated for good. Moving its times would
            // quietly bring them back to life, so an ended event's date and time are fixed (its
            // price, customer and notes can still be corrected).
            var startsAt = TenantCalendar.ToUtc(request.StartsAt);
            var endsAt = TenantCalendar.ToUtc(request.EndsAt);
            var now = DateTime.UtcNow;
            if (ev.EndsAt <= now && (Moved(ev.StartsAt, startsAt) || Moved(ev.EndsAt, endsAt)))
                throw EventRuleException.EndedTimesLocked();

            // The other way round: moving an event that hasn't ended so that it ends in the past
            // would close its issued tickets for good, and the lock above would then stop anyone
            // moving it back (e.g. a mistyped year). Refuse it while it has tickets.
            if (ev.EndsAt > now && endsAt <= now && tickets.Issued > 0)
                throw EventRuleException.EndingWouldCloseTickets(tickets.Issued);

            if (request.BranchId != ev.BranchId && !await _branchRepository.IsExistAsync(request.BranchId, cancellationToken))
                throw new BranchNotFoundException(request.BranchId.ToString());

            var customer = ev.EventCustomer;
            if (request.EventCustomerId != ev.EventCustomerId)
            {
                customer = await _customerRepository.GetByIdAsync(request.EventCustomerId, cancellationToken)
                    ?? throw new EventCustomerNotFoundException(request.EventCustomerId.ToString());
                if (!customer.IsActive)
                    throw EventRuleException.CustomerInactive();
            }

            ev.Title = request.Title.Trim();
            ev.BranchId = request.BranchId;
            ev.EventCustomerId = customer.Id;
            ev.EventCustomer = customer;
            ev.WithDecorations = request.WithDecorations;
            ev.Price = request.Price;
            ev.DecorationFee = request.WithDecorations ? request.DecorationFee : 0m;
            ev.Capacity = request.Capacity;
            // Typed in the academy's time zone; stored as UTC.
            ev.StartsAt = startsAt;
            ev.EndsAt = endsAt;
            ev.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

            await _unitOfWork.InTransactionAsync(async () =>
            {
                await _eventRepository.UpdateAsync(ev, cancellationToken);

                // The ledger re-prices the bill and refuses a total below what's been collected
                // or a branch move once money was taken at the old branch.
                if (ev.Invoice is { } invoice)
                {
                    await _financeLedgerService.ReviseEventInvoiceAsync(
                        invoice, ev, customer, request.BalanceDueDate, cancellationToken);
                }
                else
                {
                    var currency = await _currencyReader.GetCurrencyAsync(cancellationToken) ?? "KWD";
                    var issued = await _financeLedgerService.IssueEventInvoiceAsync(
                        ev, customer, currency,
                        request.BalanceDueDate ?? DateOnly.FromDateTime(TenantCalendar.ToLocal(ev.StartsAt)), cancellationToken);
                    ev.InvoiceId = issued.Id;
                    await _eventRepository.UpdateAsync(ev, cancellationToken);
                }

                return true;
            }, cancellationToken);

            var dto = await _detailsLoader.LoadAsync(ev.Id, cancellationToken);
            return Result<EventDetailsDto>.Success(dto, _operation);
        }

        // The form sends times to the minute; anything under a minute apart is the same time.
        private static bool Moved(DateTime stored, DateTime sent) => Math.Abs((stored - sent).TotalMinutes) >= 1;
    }
}
