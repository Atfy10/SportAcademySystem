using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly IPaymentRepository _paymentRepository;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IPaymentRepository paymentRepository,
            SubDetailsManagementService subscriptionDetailsMangeService)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _paymentRepository = paymentRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = request.ToSubscriptionDetails();

            await _subscriptionDetailsMangeService
                .ValidatePaymentAsync(request.PaymentNumber, cancellationToken);

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, cancellationToken);

            return Result<int>.Success(subDetails.Id, _operation);
        }
    }
}
