using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAllPaginated
{
    public class GetAllSubDetailsPaginatedQueryHandler : IRequestHandler<GetAllSubDetailsPaginatedQuery, Result<PagedData<SubscriptionDetailsDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

        public GetAllSubDetailsPaginatedQueryHandler(ISubscriptionDetailsRepository subscriptionDetailsRepository)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
        }

        public async Task<Result<PagedData<SubscriptionDetailsDto>>> Handle(GetAllSubDetailsPaginatedQuery request, CancellationToken cancellationToken)
        {
            var result = await _subscriptionDetailsRepository.GetAllPaginatedAsync(request.Page, request.Term, cancellationToken);
            return Result<PagedData<SubscriptionDetailsDto>>.Success(result, _operation);
        }
    }
}
