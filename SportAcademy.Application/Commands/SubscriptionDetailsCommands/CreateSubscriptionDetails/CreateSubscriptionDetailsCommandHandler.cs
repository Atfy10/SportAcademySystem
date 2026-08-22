using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails
{
    public class CreateSubscriptionDetailsCommandHandler : IRequestHandler<CreateSubscriptionDetailsCommand, Result<int>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly ISportPriceRepository _sportPriceRepository;
        private readonly IFinanceLedgerService _financeLedgerService;
        private readonly ITraineeRepository _traineeRepository;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;

        public CreateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService subscriptionDetailsMangeService,
            ISportPriceRepository sportPriceRepository,
            IFinanceLedgerService financeLedgerService,
            ITraineeRepository traineeRepository,
            IMapper mapper,
            IPublisher publisher)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = subscriptionDetailsMangeService;
            _sportPriceRepository = sportPriceRepository;
            _financeLedgerService = financeLedgerService;
            _traineeRepository = traineeRepository;
            _mapper = mapper;
            _publisher = publisher;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var sportPrice = await _sportPriceRepository.GetByKeyAsync(
                request.BranchId, request.SportId, request.SubscriptionTypeId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(SportPrice), $"{request.BranchId}/{request.SportId}/{request.SubscriptionTypeId}");

            var subDetails = _mapper.Map<SubscriptionDetails>(request)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            var isSubActive = SubscriptionDetailsService.IsSubscriptionActive(subDetails);
            if (!isSubActive)
                subDetails.Status = SubscriptionStatus.Expired;

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.AddAsync(subDetails, cancellationToken);

            // Billing is a deliberate, separate act from money changing hands - creating a
            // subscription issues an Invoice; recording an actual Payment against it is done
            // later through the Accountant console (see FinanceController.RecordPayment).
            await _financeLedgerService.IssueSubscriptionInvoiceAsync(
                subDetails, sportPrice.Price, "KWD", cancellationToken);

            await _publisher.Publish(new SubscriptionCreatedEvent(subDetails.Id, subDetails.TraineeId), cancellationToken);

            return Result<int>.Success(subDetails.Id, _operation);
        }
    }
}
