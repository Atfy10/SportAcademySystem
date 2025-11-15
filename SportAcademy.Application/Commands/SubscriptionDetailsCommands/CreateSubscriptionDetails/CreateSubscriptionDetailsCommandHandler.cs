using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.PaymentExceptions;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Services;
using System.Threading;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IPaymentRepository paymentRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            IMapper mapper)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _paymentRepository = paymentRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = _mapper.Map<SubscriptionDetails>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _subscriptionDetailsMangeService
                .ValidatePaymentAsync(request.PaymentNumber, cancellationToken);

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            var isSubActive = SubscriptionDetailsService.IsSubscriptionActive(subDetails);
            if (!isSubActive)
                subDetails.IsActive = false;

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, cancellationToken);

            return Result<int>.Success(subDetails.Id, _operation);
        }
    }
}
