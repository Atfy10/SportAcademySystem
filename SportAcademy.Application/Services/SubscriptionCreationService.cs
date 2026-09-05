using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Services
{
    // Extracted verbatim out of CreateSubscriptionDetailsCommandHandler - see
    // ISubscriptionCreationService for why. Behavior for discountAmount: 0/discountCodeId: null
    // (the plain path) must stay identical to what the handler did inline before this extraction.
    public class SubscriptionCreationService : ISubscriptionCreationService
    {
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly ISportPriceRepository _sportPriceRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ISportTraineeRepository _sportTraineeRepository;
        private readonly IPublisher _publisher;

        public SubscriptionCreationService(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            ISportPriceRepository sportPriceRepository,
            IFinanceLedgerService financeLedgerService,
            IEnrollmentRepository enrollmentRepository,
            ISportTraineeRepository sportTraineeRepository,
            IPublisher publisher)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _sportPriceRepository = sportPriceRepository;
            _financeLedgerService = financeLedgerService;
            _enrollmentRepository = enrollmentRepository;
            _sportTraineeRepository = sportTraineeRepository;
            _publisher = publisher;
        }

        public async Task<SubscriptionDetails> CreateAsync(
            int traineeId, int subscriptionTypeId, int sportId, int branchId,
            DateOnly startDate, TraineeGroupType groupType, IReadOnlyCollection<DayOfWeek> trainingDays,
            int paymentTypeId,
            decimal? discountPercentage, int? discountCodeId, Guid? actingUserId,
            CancellationToken ct = default)
        {
            var sportPrice = await _sportPriceRepository.GetByKeyWithIncludesAsync(
                branchId, sportId, subscriptionTypeId, groupType, ct)
                ?? throw new IdNotFoundException(nameof(SportPrice), $"{branchId}/{sportId}/{subscriptionTypeId}/{groupType}");

            var discountAmount = discountPercentage.HasValue
                ? Math.Round(sportPrice.Price * discountPercentage.Value / 100m, 3)
                : 0m;

            // How long the subscription runs depends on the training days as much as on the plan:
            // the same number of sessions takes longer to use up at 2 days a week than at 3. The
            // date is counted across the chosen pattern rather than added as a flat calendar
            // duration, and the group picked later is constrained to these same days - so the
            // billing period and the trainee's real schedule stay in step.
            var subscriptionType = sportPrice.SportSubscriptionType.SubscriptionType;
            var totalSessions = TrainingScheduleService.CalculateTotalSessions(
                subscriptionType.DaysPerMonth, subscriptionType.NumberOfMonths);
            var endDate = TrainingScheduleService.ComputeEndDate(startDate, totalSessions, trainingDays);

            var subDetails = new SubscriptionDetails
            {
                StartDate = startDate,
                EndDate = endDate,
                TraineeId = traineeId,
                SubscriptionTypeId = subscriptionTypeId,
                SportId = sportId,
                BranchId = branchId,
                GroupType = groupType,
                TrainingDays = trainingDays.Distinct().OrderBy(d => d).ToList(),
            };

            await _subscriptionDetailsMangeService.ValidateSubscriptionAsync(subDetails, ct);

            if (SubscriptionDetailsService.HasExpired(subDetails))
                subDetails.Status = SubscriptionStatus.Expired;

            ct.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, ct);

            // Subscribing is often how a trainee starts a brand-new sport, so it must not require
            // them to already have a SportTrainee record for sportId. Back-fill one here with an
            // unset skill level (NotSpecified) if they don't have it yet - CreateEnrollment's
            // skill-level gate already treats NotSpecified the same as "no record at all", so
            // this doesn't block them from later enrolling into any group for this sport.
            var hasSportRecord = await _sportTraineeRepository.IsExistAsync(sportId, traineeId, ct);
            if (!hasSportRecord)
            {
                await _sportTraineeRepository.AddAsync(new SportTrainee
                {
                    SportId = sportId,
                    TraineeId = traineeId,
                    SkillLevel = SkillLevel.NotSpecified
                }, ct);
            }

            // A trainee can only be enrolled in one group per sport - if they already have a
            // group enrollment for this sport (i.e. this is a renewal, not a first-time sign-up),
            // carry that enrollment forward onto the new subscription instead of leaving it
            // pointing at the now-superseded one. No manual re-enrollment step needed.
            // First-time subscriptions (no existing enrollment) are untouched.
            var existingEnrollment = await _enrollmentRepository.GetCurrentEnrollmentForSportAsync(traineeId, sportId, ct);
            if (existingEnrollment is not null)
            {
                var oldSubscriptionDetailsId = existingEnrollment.SubscriptionDetailsId;

                // Not SubscriptionDetailsService.CalculateAllowedSessions(subDetails) - that
                // reads subDetails.SportPrice.SportSubscriptionType.SubscriptionType, which is
                // null on this freshly-created-and-added entity (no navigations loaded/attached).
                // sportPrice was fetched with includes specifically for this.
                existingEnrollment.SubscriptionDetailsId = subDetails.Id;
                existingEnrollment.SessionAllowed = TrainingScheduleService.CalculateTotalSessions(
                    sportPrice.SportSubscriptionType.SubscriptionType.DaysPerMonth,
                    sportPrice.SportSubscriptionType.SubscriptionType.NumberOfMonths);
                existingEnrollment.SessionRemaining = existingEnrollment.SessionAllowed;
                existingEnrollment.ExpiryDate = subDetails.EndDate.ToDateTime(TimeOnly.MinValue);
                existingEnrollment.IsActive = true;
                await _enrollmentRepository.UpdateAsync(existingEnrollment, ct);

                // Expire the superseded subscription immediately rather than waiting for the lazy
                // status flip in GetSubDetailsStatsAsync - otherwise it keeps showing as Active
                // until something else happens to query subscription stats.
                var oldSubscription = await _subscriptionDetailsRepository.GetByIdAsync(oldSubscriptionDetailsId, ct);
                if (oldSubscription is not null && oldSubscription.Status != SubscriptionStatus.Expired)
                {
                    oldSubscription.Status = SubscriptionStatus.Expired;
                    await _subscriptionDetailsRepository.UpdateAsync(oldSubscription, ct);
                }
            }

            // Subscriptions are typically paid for at the point of sale, so creation issues an
            // Invoice and immediately records a full payment against it via the chosen method -
            // not a deferred Accountant-only step. discountAmount/discountCodeId are 0/null on
            // the plain (no discount code) path.
            var invoice = await _financeLedgerService.IssueSubscriptionInvoiceAsync(
                subDetails, sportPrice.Price, discountAmount, discountCodeId, "KWD", ct);

            await _financeLedgerService.RecordPaymentAsync(new RecordPaymentInput(
                Amount: invoice.GrandTotal,
                PaymentTypeId: paymentTypeId,
                BranchId: branchId,
                Currency: "KWD",
                Reference: null,
                Notes: null,
                RecordedByUserId: actingUserId,
                Allocations: [new PaymentAllocationInput(invoice.Id, invoice.GrandTotal)]
            ), ct);

            await _publisher.Publish(new SubscriptionCreatedEvent(subDetails.Id, subDetails.TraineeId), ct);

            return subDetails;
        }
    }
}
