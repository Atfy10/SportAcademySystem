using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Commands.SubscriptionType.CreateSubscriptionType
{
    public class CreateSubscriptionTypeCommandHandler : IRequestHandler<CreateSubscriptionTypeCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ISubscriptionTypeRepository _subscriptionTypeRepository;
        public CreateSubscriptionTypeCommandHandler(ISubscriptionTypeRepository subscriptionTypeRepository)
        {
            _subscriptionTypeRepository = subscriptionTypeRepository;
        }
        public async Task<Result<int>> Handle(CreateSubscriptionTypeCommand request, CancellationToken cancellationToken)
        {
            var subscriptionType = new SportAcademy.Domain.Entities.SubscriptionType
            {
                Name = request.Name,
                DaysPerMonth = request.DaysPerMonth,
                IsActive = request.IsActive,
                IsOffer = request.IsOffer
            };

            await _subscriptionTypeRepository.AddAsync(subscriptionType, cancellationToken);

            return   Result<int>.Success(subscriptionType.Id, _operationType);
        }
    }
}
