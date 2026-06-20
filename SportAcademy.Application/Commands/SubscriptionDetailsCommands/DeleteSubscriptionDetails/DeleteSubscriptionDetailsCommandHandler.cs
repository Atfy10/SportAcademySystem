using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails
{
    public class DeleteSubscriptionDetailsCommandHandler : IRequestHandler<DeleteSubscriptionDetailsCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

        public DeleteSubscriptionDetailsCommandHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<bool>> Handle(DeleteSubscriptionDetailsCommand request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new SubscriptionTypeNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            await _subscriptionDetailsRepository.DeleteAsync(subDetails, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return Result<bool>.Success(true, _operation);
        }
    }
}
