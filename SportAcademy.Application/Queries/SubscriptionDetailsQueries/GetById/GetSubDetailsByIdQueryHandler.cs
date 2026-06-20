using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetById
{
    public class GetSubDetailsByIdQueryHandler : IRequestHandler<GetSubDetailsByIdQuery, Result<SubscriptionDetailsDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

        public GetSubDetailsByIdQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<SubscriptionDetailsDto>> Handle(GetSubDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetFullSubscriptionDetails(request.Id)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var subDetailsDto = subDetails.ToDto();

            cancellationToken.ThrowIfCancellationRequested();

            return Result<SubscriptionDetailsDto>.Success(subDetailsDto, _operation);
        }
    }
}
