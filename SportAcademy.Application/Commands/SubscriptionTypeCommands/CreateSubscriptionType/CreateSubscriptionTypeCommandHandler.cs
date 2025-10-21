using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Entities;
using AutoMapper;

namespace SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType
{
    public class CreateSubscriptionTypeCommandHandler : IRequestHandler<CreateSubscriptionTypeCommand, Result<int>>
    {
        private readonly string _operationType = OperationType.Add.ToString();
        private readonly ISubscriptionTypeRepository _subscriptionTypeRepository;
        private readonly IMapper _mapper;

        public CreateSubscriptionTypeCommandHandler(
            ISubscriptionTypeRepository subscriptionTypeRepository,
            IMapper mapper)
        {
            _subscriptionTypeRepository = subscriptionTypeRepository;
            _mapper = mapper;
        }
        public async Task<Result<int>> Handle(CreateSubscriptionTypeCommand request, CancellationToken cancellationToken)
        {
            var subscriptionType = _mapper.Map<SubscriptionType>(request);

            await _subscriptionTypeRepository.AddAsync(subscriptionType, cancellationToken);

            return Result<int>.Success(subscriptionType.Id, _operationType);
        }
    }
}
