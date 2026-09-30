using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Application.Services
{
    public class SubDetailsManagementService
    {
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

        public SubDetailsManagementService(ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        // Billing (issuing an Invoice for the new subscription) is handled separately by
        // IFinanceLedgerService - it used to be fabricated here as a side-effect Payment
        // regardless of whether money had actually changed hands, which the Invoice/Payment
        // split corrects.
        public async Task ValidateSubscriptionAsync(SubscriptionDetails sub, CancellationToken ct)
        {
            // Every one of the trainee's subscriptions (any status), not just Active ones: a
            // suspended or upcoming subscription still owns its dates, and the old Active-only
            // check let a new subscription start before - or inside - an upcoming one.
            var existing = await _subscriptionDetailsRepository
                .GetSubscriptionDetailsForTraineeAsync(sub.TraineeId, ct);

            var conflict = SubscriptionDetailsService.FindSameSportConflict(sub, existing);
            if (conflict is not null)
                throw new SubscriptionConflictException(conflict.EndDate);
        }
    }
}
