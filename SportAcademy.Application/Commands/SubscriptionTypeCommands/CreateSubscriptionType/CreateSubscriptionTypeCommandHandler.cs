using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType
{
    public class CreateSubscriptionTypeCommandHandler : IRequestHandler<CreateSubscriptionTypeCommand, Result<int>>
    {
        private readonly string _operation = OperationType.Add.ToString();
        private readonly ISubscriptionTypeRepository _repository;

        public CreateSubscriptionTypeCommandHandler(ISubscriptionTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<int>> Handle(CreateSubscriptionTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = request.ToEntity();

            await _repository.AddAsync(entity, cancellationToken);

            return Result<int>.Success(entity.Id, _operation);
        }
    }
}