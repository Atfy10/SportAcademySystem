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
            var activeSubs = await _subscriptionDetailsRepository
                .GetActiveSubscriptionDetailsForTraineeAsync(sub.TraineeId, ct);

            var hasConflict = SubscriptionDetailsService.HasActiveSubscriptionConflict(
                sub, activeSubs);
            if (hasConflict)
                throw new SubscriptionConflictException();
        }
    }
}
