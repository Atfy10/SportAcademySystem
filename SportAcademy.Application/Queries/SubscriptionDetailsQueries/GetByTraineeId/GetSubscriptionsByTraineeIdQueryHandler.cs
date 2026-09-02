using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetByTraineeId
{
    public class GetSubscriptionsByTraineeIdQueryHandler : IRequestHandler<GetSubscriptionsByTraineeIdQuery, Result<List<SubscriptionDetailsDto>>>
    {
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetSubscriptionsByTraineeIdQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IMapper mapper)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<SubscriptionDetailsDto>>> Handle(GetSubscriptionsByTraineeIdQuery request, CancellationToken cancellationToken)
        {
            var subscriptions = await _subscriptionDetailsRepository.GetAllFullSubDetailsForTraineeIdAsync(request.TraineeId, cancellationToken);
            var dtos = _mapper.Map<List<SubscriptionDetailsDto>>(subscriptions);
            return Result<List<SubscriptionDetailsDto>>.Success(dtos, _operationType);
        }
    }
}
