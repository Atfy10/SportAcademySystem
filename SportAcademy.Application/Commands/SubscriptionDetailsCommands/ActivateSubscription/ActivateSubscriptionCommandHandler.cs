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

        public ActivateSubscriptionCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<bool>> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            subDetails.Status = SubscriptionStatus.Active;

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operation);
        }
    }
}
