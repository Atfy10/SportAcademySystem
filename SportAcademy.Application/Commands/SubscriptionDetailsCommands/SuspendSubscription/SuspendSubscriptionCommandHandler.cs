using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.SuspendSubscription
{
    public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Update.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

        public SuspendSubscriptionCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<bool>> Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            if (subDetails.Status is SubscriptionStatus.Expired)
                return Result<bool>.Failure(_operation, "Expired subscription can not be suspended.");

            subDetails.Status = SubscriptionStatus.Suspended;

            await _subscriptionDetailsRepository.UpdateAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operation);
        }
    }
}
