using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.ActivateSubscription
{
    public class ActivateSubscriptionCommandHandler : IRequestHandler<ActivateSubscriptionCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly ITraineeRepository _traineeRepository;

        public ActivateSubscriptionCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            ITraineeRepository traineeRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _traineeRepository = traineeRepository;
        }

        public async Task<Result<bool>> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            subDetails.Status = SubscriptionStatus.Active;

            var traineeId = subDetails.TraineeId;

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            await _traineeRepository.RecalculateIsSubscribedAsync(traineeId, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}
