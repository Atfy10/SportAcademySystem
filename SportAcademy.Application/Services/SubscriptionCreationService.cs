using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Finance;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.PaymentExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Services
{
    // Extracted verbatim out of CreateSubscriptionDetailsCommandHandler - see
    // ISubscriptionCreationService for why.
    public class SubscriptionCreationService : ISubscriptionCreationService
    {
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly ISportPriceRepository _sportPriceRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ISportTraineeRepository _sportTraineeRepository;
        private readonly IPublisher _publisher;
        private readonly ITenantSettingsCurrencyReader _currencyReader;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionCreationService(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            ISportPriceRepository sportPriceRepository,
            IFinanceLedgerService financeLedgerService,
            IEnrollmentRepository enrollmentRepository,
            ISportTraineeRepository sportTraineeRepository,
            IPublisher publisher,
            ITenantSettingsCurrencyReader currencyReader,
            IUnitOfWork unitOfWork)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _sportPriceRepository = sportPriceRepository;
            _financeLedgerService = financeLedgerService;
            _enrollmentRepository = enrollmentRepository;
            _sportTraineeRepository = sportTraineeRepository;
            _publisher = publisher;
            _currencyReader = currencyReader;
            _unitOfWork = unitOfWork;
        }

        public async Task<SubscriptionCreationResult> CreateAsync(
            SubscriptionCreationRequest request,
            Func<SubscriptionCreationResult, CancellationToken, Task>? beforeCommit = null,
            CancellationToken ct = default)
        {
            var sportPrice = await _sportPriceRepository.GetByKeyWithIncludesAsync(
                request.BranchId, request.SportId, request.SubscriptionTypeId, request.GroupType, ct)
                ?? throw new IdNotFoundException(nameof(SportPrice),
                    $"{request.BranchId}/{request.SportId}/{request.SubscriptionTypeId}/{request.GroupType}");

            var discountAmount = request.DiscountPercentage.HasValue
                ? Math.Round(sportPrice.Price * request.DiscountPercentage.Value / 100m, 3)
                : 0m;
            var netPrice = sportPrice.Price - Math.Min(discountAmount, sportPrice.Price);

            // Checked here (not only in the validator) because the discount-request path only
            // learns the discounted total at approval time.
            if (request.DepositAmount is { } requestedDeposit && netPrice > 0 && requestedDeposit >= netPrice)
                throw FinanceRuleException.DepositNotLessThanTotal(netPrice);

            // How long the subscription runs depends on the training days as much as on the plan:
            // the same number of sessions takes longer to use up at 2 days a week than at 3. The
            // date is counted across the chosen pattern rather than added as a flat calendar
            // duration, and the group picked later is constrained to these same days - so the
            // billing period and the trainee's real schedule stay in step.
            var subscriptionType = sportPrice.SportSubscriptionType.SubscriptionType;
            var totalSessions = TrainingScheduleService.CalculateTotalSessions(
                subscriptionType.DaysPerMonth, subscriptionType.NumberOfMonths);
            var endDate = TrainingScheduleService.ComputeEndDate(request.StartDate, totalSessions, request.TrainingDays);

            var subDetails = new SubscriptionDetails
            {
                StartDate = request.StartDate,
                EndDate = endDate,
                TraineeId = request.TraineeId,
                SubscriptionTypeId = request.SubscriptionTypeId,
                SportId = request.SportId,
                BranchId = request.BranchId,
                GroupType = request.GroupType,
                TrainingDays = request.TrainingDays.Distinct().OrderBy(d => d).ToList(),
            };

            await _subscriptionDetailsMangeService.ValidateSubscriptionAsync(subDetails, ct);

            if (SubscriptionDetailsService.HasExpired(subDetails))
                subDetails.Status = SubscriptionStatus.Expired;

            ct.ThrowIfCancellationRequested();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            SubscriptionCreationResult result;

            // Everything below is several independent SaveChanges calls (subDetails, the
            // back-filled SportTrainee, the carried-forward enrollment, the superseded
            // subscription's expiry, the invoice, the payment) - without an explicit transaction
            // a failure partway through left earlier steps already committed: an orphaned
            // SubscriptionDetails row with no invoice/payment, while the UI reported the whole
            // operation as failed. Wrapped in one transaction so a failure anywhere rolls back
            // everything, matching AcceptInvitationCommandHandler's pattern.
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                await _subscriptionDetailsRepository.AddAsync(subDetails, ct);

                // Subscribing is often how a trainee starts a brand-new sport, so it must not
                // require them to already have a SportTrainee record for the sport. Back-fill one
                // here with an unset skill level (NotSpecified) if they don't have it yet -
                // CreateEnrollment's skill-level gate already treats NotSpecified the same as "no
                // record at all", so this doesn't block them from later enrolling into any group
                // for this sport.
                var hasSportRecord = await _sportTraineeRepository.IsExistAsync(request.SportId, request.TraineeId, ct);
                if (!hasSportRecord)
                {
                    await _sportTraineeRepository.AddAsync(new SportTrainee
                    {
                        SportId = request.SportId,
                        TraineeId = request.TraineeId,
                        SkillLevel = SkillLevel.NotSpecified
                    }, ct);
                }

                // A trainee can only be enrolled in one group per sport - if they already have a
                // group enrollment for this sport (i.e. this is a renewal, not a first-time
                // sign-up), carry that enrollment forward onto the new subscription.
                //
                // Only when the renewal has already started, though. A renewal booked ahead of
                // time (start date in the future) must leave the trainee training on - and
                // consuming the sessions of - the subscription they're on now; resetting the
                // enrollment today would throw away the sessions they already paid for.
                // SubscriptionLifecycleService does the hand-over on the renewal's start date.
                if (request.StartDate <= today)
                {
                    var existingEnrollment = await _enrollmentRepository.GetCurrentEnrollmentForSportAsync(
                        request.TraineeId, request.SportId, ct);
                    if (existingEnrollment is not null)
                        await HandOverEnrollmentAsync(existingEnrollment, subDetails, totalSessions, ct);
                }

                var currency = await _currencyReader.GetCurrencyAsync(ct) ?? "KWD";

                // Paid in full: due today and settled right away. With a deposit: due on the
                // collect date the user picked (a week out if, somehow, none was given).
                var dueDate = request.DepositAmount.HasValue
                    ? request.BalanceDueDate ?? today.AddDays(7)
                    : today;

                var invoice = await _financeLedgerService.IssueSubscriptionInvoiceAsync(
                    subDetails, sportPrice.Price, discountAmount, request.DiscountCodeId, currency, dueDate, ct);

                // Subscriptions are typically paid for at the point of sale, so creation records
                // the payment immediately via the chosen method - the whole total, or just the
                // deposit. A zero-total invoice (100% discount) is already Paid; there's nothing to
                // record.
                Payment? payment = null;
                var amountNow = request.DepositAmount ?? invoice.GrandTotal;
                if (invoice.GrandTotal > 0 && amountNow > 0)
                {
                    payment = await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                        Amount: amountNow,
                        PaymentTypeId: request.PaymentTypeId,
                        BranchId: request.BranchId,
                        Currency: currency,
                        Reference: null,
                        // Exactly what the user wrote (shown on the receipt and the Payments
                        // list); no invented English label when they wrote nothing.
                        Notes: string.IsNullOrWhiteSpace(request.PaymentNote) ? null : request.PaymentNote.Trim(),
                        RecordedByUserId: request.ActingUserId,
                        Allocations: [new PaymentAllocationInput(invoice.Id, amountNow)]
                    ), ct);
                }

                result = new SubscriptionCreationResult(subDetails, invoice, payment);

                if (beforeCommit is not null)
                    await beforeCommit(result, ct);

                await _unitOfWork.CommitTransactionAsync(ct);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }

            await _publisher.Publish(new SubscriptionCreatedEvent(subDetails.Id, subDetails.TraineeId), ct);

            return result;
        }

        private async Task HandOverEnrollmentAsync(
            Enrollment existingEnrollment, SubscriptionDetails subDetails, int totalSessions, CancellationToken ct)
        {
            var oldSubscriptionDetailsId = existingEnrollment.SubscriptionDetailsId;

            existingEnrollment.SubscriptionDetailsId = subDetails.Id;
            existingEnrollment.SessionAllowed = totalSessions;
            existingEnrollment.SessionRemaining = totalSessions;
            existingEnrollment.ExpiryDate = subDetails.EndDate.ToDateTime(TimeOnly.MinValue);
            existingEnrollment.Status = EnrollmentStatus.Active;
            await _enrollmentRepository.UpdateAsync(existingEnrollment, ct);

            // Expire the superseded subscription immediately rather than waiting for the daily
            // lifecycle sweep - otherwise it keeps showing as Active until then.
            var oldSubscription = await _subscriptionDetailsRepository.GetByIdAsync(oldSubscriptionDetailsId, ct);
            if (oldSubscription is not null && oldSubscription.Id != subDetails.Id
                && oldSubscription.Status != SubscriptionStatus.Expired)
            {
                oldSubscription.Status = SubscriptionStatus.Expired;
                await _subscriptionDetailsRepository.UpdateAsync(oldSubscription, ct);
            }
        }
    }
}
