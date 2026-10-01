using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EventDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Events;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.EventExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Commands.EventCommands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<EventDetailsDto>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly IEventRepository _eventRepository;
        private readonly IEventCustomerRepository _customerRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly INationalityCategoryRepository _nationalityCategoryRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly ITenantSettingsCurrencyReader _currencyReader;
        private readonly IPhoneNumberNormalizer _phoneNormalizer;
        private readonly IUserContextService _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EventDetailsLoader _detailsLoader;

        public CreateEventCommandHandler(
            IEventRepository eventRepository,
            IEventCustomerRepository customerRepository,
            IBranchRepository branchRepository,
            INationalityCategoryRepository nationalityCategoryRepository,
            IFinanceLedgerService financeLedgerService,
            ITenantSettingsCurrencyReader currencyReader,
            IPhoneNumberNormalizer phoneNormalizer,
            IUserContextService userContext,
            IUnitOfWork unitOfWork,
            EventDetailsLoader detailsLoader)
        {
            _eventRepository = eventRepository;
            _customerRepository = customerRepository;
            _branchRepository = branchRepository;
            _nationalityCategoryRepository = nationalityCategoryRepository;
            _financeLedgerService = financeLedgerService;
            _currencyReader = currencyReader;
            _phoneNormalizer = phoneNormalizer;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
            _detailsLoader = detailsLoader;
        }

        public async Task<Result<EventDetailsDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            // Who booked the event is part of the record - never fall back to Guid.Empty.
            if (_userContext.UserId is not { } userId || userId == Guid.Empty)
                throw EventRuleException.NoActingUser();

            if (!await _branchRepository.IsExistAsync(request.BranchId, cancellationToken))
                throw new BranchNotFoundException(request.BranchId.ToString());

            var decorationFee = request.WithDecorations ? request.DecorationFee : 0m;
            var total = request.Price + decorationFee;
            if (request.AmountPaidNow > total)
                throw EventRuleException.PaymentExceedsTotal(total);

            var (customer, isNewCustomer) = await ResolveCustomerAsync(request, cancellationToken);

            var ev = new Event
            {
                Title = request.Title.Trim(),
                BranchId = request.BranchId,
                WithDecorations = request.WithDecorations,
                Price = request.Price,
                DecorationFee = decorationFee,
                Capacity = request.Capacity,
                // Typed in the academy's time zone; stored as UTC.
                StartsAt = TenantCalendar.ToUtc(request.StartsAt),
                EndsAt = TenantCalendar.ToUtc(request.EndsAt),
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                CreatedByUserId = userId,
            };

            // Customer, event, invoice and the first payment are separate SaveChanges calls -
            // one transaction, so a failure anywhere leaves no half-booked event or orphan bill.
            await _unitOfWork.InTransactionAsync(async () =>
            {
                if (isNewCustomer)
                    customer = await AddOrReuseCustomerAsync(customer, cancellationToken);

                ev.EventCustomerId = customer.Id;
                await _eventRepository.AddAsync(ev, cancellationToken);

                var currency = await _currencyReader.GetCurrencyAsync(cancellationToken) ?? "KWD";
                var today = TenantCalendar.Today;
                var paidInFull = request.AmountPaidNow >= total;
                var dueDate = paidInFull
                    ? today
                    : request.BalanceDueDate ?? Max(today, DateOnly.FromDateTime(TenantCalendar.ToLocal(ev.StartsAt)));

                var invoice = await _financeLedgerService.IssueEventInvoiceAsync(ev, customer, currency, dueDate, cancellationToken);

                ev.InvoiceId = invoice.Id;
                await _eventRepository.UpdateAsync(ev, cancellationToken);

                if (request.AmountPaidNow > 0 && invoice.GrandTotal > 0)
                {
                    await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                        Amount: request.AmountPaidNow,
                        PaymentTypeId: request.PaymentTypeId!.Value,
                        BranchId: ev.BranchId,
                        Currency: currency,
                        Reference: null,
                        Notes: string.IsNullOrWhiteSpace(request.PaymentNote) ? null : request.PaymentNote.Trim(),
                        RecordedByUserId: userId,
                        Allocations: [new PaymentAllocationInput(invoice.Id, request.AmountPaidNow)]
                    ), cancellationToken);
                }

                return true;
            }, cancellationToken);

            var dto = await _detailsLoader.LoadAsync(ev.Id, cancellationToken);
            return Result<EventDetailsDto>.Success(dto, _operation);
        }

        // An existing customer picked on the form, or one typed in. A typed-in phone number that
        // already belongs to a customer (e.g. two people booking for the same person at once)
        // books that existing customer rather than failing or overwriting their details.
        private async Task<(EventCustomer Customer, bool IsNew)> ResolveCustomerAsync(
            CreateEventCommand request, CancellationToken ct)
        {
            EventCustomer? customer;
            if (request.EventCustomerId is { } customerId)
            {
                customer = await _customerRepository.GetByIdAsync(customerId, ct)
                    ?? throw new EventCustomerNotFoundException(customerId.ToString());
            }
            else
            {
                var input = request.NewCustomer!;
                var phone = await _phoneNormalizer.NormalizeAsync(input.PhoneNumber, ct) ?? input.PhoneNumber.Trim();
                customer = await _customerRepository.GetByPhoneAsync(phone, ct);

                if (customer is null)
                {
                    if (!await _nationalityCategoryRepository.IsExistAsync(input.NationalityCategoryId, ct))
                        throw new IdNotFoundException(nameof(NationalityCategory), input.NationalityCategoryId);

                    return (new EventCustomer
                    {
                        FullName = input.FullName.Trim(),
                        PhoneNumber = phone,
                        NationalityCategoryId = input.NationalityCategoryId,
                    }, true);
                }
            }

            if (!customer.IsActive)
                throw EventRuleException.CustomerInactive();

            return (customer, false);
        }

        // Someone else may have saved a customer with this phone between our lookup and this insert
        // (two people booking for the same new customer at once): the unique (tenant, phone) index
        // rejects ours, and we book theirs instead - the same "existing phone wins" rule as above.
        // A unique-key error doesn't end the surrounding SQL Server transaction, so it carries on.
        private async Task<EventCustomer> AddOrReuseCustomerAsync(EventCustomer customer, CancellationToken ct)
        {
            try
            {
                await _customerRepository.AddAsync(customer, ct);
                return customer;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                _unitOfWork.ClearChangeTracker();
                var existing = await _customerRepository.GetByPhoneAsync(customer.PhoneNumber, ct);
                if (existing is null) throw;
                if (!existing.IsActive) throw EventRuleException.CustomerInactive();
                return existing;
            }
        }

        private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;
    }
}
