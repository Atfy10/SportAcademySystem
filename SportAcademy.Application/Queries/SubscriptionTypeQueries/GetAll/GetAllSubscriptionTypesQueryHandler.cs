using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionTypeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionTypeQueries.GetAll
{
    public class GetAllSubscriptionTypesQueryHandler : IRequestHandler<GetAllSubscriptionTypesQuery, Result<List<SubscriptionTypeDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly ISubscriptionTypeRepository _repository;

        public GetAllSubscriptionTypesQueryHandler(ISubscriptionTypeRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<SubscriptionTypeDto>>> Handle(GetAllSubscriptionTypesQuery request, CancellationToken cancellationToken)
        {
            var entities = await _repository.GetAllWithSportsAsync(cancellationToken);

            var dtos = entities.Select(e => e.ToDto()).ToList();

            return Result<List<SubscriptionTypeDto>>.Success(dtos, _operation);
        }
    }
}