using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.PaymentExceptions;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SportAcademy.Application.Services
{
    public class SubDetailsManagementService
    {
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IPaymentRepository _paymentRepository;

        public SubDetailsManagementService(IPaymentRepository paymentRepository,
                                         ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _paymentRepository = paymentRepository;
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task ValidatePaymentAsync(string? paymentNumber, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(paymentNumber))
                throw new PaymentNotFoundException(nameof(paymentNumber));

            var isPaymentExist = await _paymentRepository.IsExistAsync(paymentNumber,
                    ct);
            if (!isPaymentExist)
                throw new PaymentNotFoundException(paymentNumber);

            var isPaymentRelatedToSub = await _paymentRepository.IsRelatedToSubscriptionAsync(
                paymentNumber, ct);
            if (isPaymentRelatedToSub)
                throw new PaymentConflictException(paymentNumber);
        }

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
