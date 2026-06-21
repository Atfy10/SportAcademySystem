using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionTypeQueries.GetById
{
    public class GetSubscriptionTypeByIdQueryHandler : IRequestHandler<GetSubscriptionTypeByIdQuery, Result<SubscriptionTypeDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly ISubscriptionTypeRepository _repository;

        public GetSubscriptionTypeByIdQueryHandler(ISubscriptionTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<SubscriptionTypeDto>> Handle(GetSubscriptionTypeByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _repository.GetByIdWithSportsAsync(request.Id, cancellationToken);

            if (entity is null)
                return Result<SubscriptionTypeDto>.Failure($"Subscription type with ID {request.Id} not found.", _operation, 404);

            var dto = entity.ToDto();
            return Result<SubscriptionTypeDto>.Success(dto, _operation);
        }
    }
}