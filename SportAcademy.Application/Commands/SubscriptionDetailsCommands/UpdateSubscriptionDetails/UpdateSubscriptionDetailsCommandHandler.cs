using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails
{
    public class UpdateSubscriptionDetailsCommandHandler : IRequestHandler<UpdateSubscriptionDetailsCommand, Result<SubscriptionDetailsDto>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;

        public UpdateSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
        }

        public async Task<Result<SubscriptionDetailsDto>> Handle(UpdateSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            subDetails.UpdateDates(request.StartDate, request.EndDate);
            subDetails.UpdatePayment(request.PaymentNumber);

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsMangeService
                .ValidatePaymentAsync(request.PaymentNumber, cancellationToken);

            await _subscriptionDetailsMangeService
                .ValidateSubscriptionAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            var subscriptionDetailsDto = subDetails.ToDto();

            return Result<SubscriptionDetailsDto>.Success(subscriptionDetailsDto, _operation);
        }
    }
}
