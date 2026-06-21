using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.DeleteSubscriptionType
{
    public class DeleteSubscriptionTypeCommandHandler : IRequestHandler<DeleteSubscriptionTypeCommand, Result<bool>>
    {
        private readonly string _operation = OperationType.Delete.ToString();
        private readonly ISubscriptionTypeRepository _repository;

        public DeleteSubscriptionTypeCommandHandler(ISubscriptionTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> Handle(DeleteSubscriptionTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<bool>.Failure($"Subscription type with ID {request.Id} not found.", _operation, 404);

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true, _operation);
        }
    }
}