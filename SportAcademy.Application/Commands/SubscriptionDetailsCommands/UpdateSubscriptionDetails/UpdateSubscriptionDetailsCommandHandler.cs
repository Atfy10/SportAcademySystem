using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using SportAcademy.Domain.Services;
using System.Buffers;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails
{
    public class UpdateSubscriptionDetailsCommandHandler : IRequestHandler<UpdateSubscriptionDetailsCommand, Result<SubscriptionDetailsDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;
        private readonly ITraineeRepository _traineeRepository;

        public UpdateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService,
            IMapper mapper,
            ITraineeRepository traineeRepository
            )
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
            _mapper = mapper;
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<SubscriptionDetailsDto>> Handle(UpdateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            _mapper.Map(request, subDetails);

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsMangeService
                .ValidatePaymentAsync(request.PaymentNumber, cancellationToken);

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            var isSubActive = SubscriptionDetailsService.IsSubscriptionActive(subDetails);
            if (!isSubActive)
                subDetails.Status = SubscriptionStatus.Expired;

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            await _traineeRepository.RecalculateIsSubscribedAsync(subDetails.TraineeId, cancellationToken);

            var subscriptionDetailsDto = _mapper.Map<SubscriptionDetailsDto>(subDetails);

            return Result<SubscriptionDetailsDto>.Success(subscriptionDetailsDto, _operation);
        }
    }
}
