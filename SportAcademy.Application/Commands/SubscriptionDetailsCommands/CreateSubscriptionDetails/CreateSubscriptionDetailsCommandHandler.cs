using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            IMapper mapper,
            IPublisher publisher)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _mapper = mapper;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var paymentNumber = await _subscriptionDetailsMangeService
                .EnsurePaymentAsync(request.PaymentNumber, request.BranchId, cancellationToken);
            request = request with { PaymentNumber = paymentNumber };

            var subDetails = _mapper.Map<SubscriptionDetails>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            var isSubActive = SubscriptionDetailsService.IsSubscriptionActive(subDetails);
            if (!isSubActive)
                subDetails.Status = SubscriptionStatus.Expired;

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, cancellationToken);

            await _publisher.Publish(new SubscriptionCreatedEvent(subDetails.Id, subDetails.TraineeId), cancellationToken);

            return Result<int>.Success(subDetails.Id, _operation);
        }
    }
}
