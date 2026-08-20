using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetLatest
{
    public class GetLatestSubDetailsQueryHandler : IRequestHandler<GetLatestSubDetailsQuery, Result<PagedData<SubscriptionDetailsDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;

        public GetLatestSubDetailsQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IMapper mapper)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _mapper = mapper;
        }

        public async Task<Result<PagedData<SubscriptionDetailsDto>>> Handle(GetLatestSubDetailsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _subscriptionDetailsRepository.GetLatestSubscriptionsAsync(
                request.Page, request.Term, cancellationToken);

            var dtoItems = _mapper.Map<List<SubscriptionDetailsDto>>(items) ?? [];

            var pagedData = new PagedData<SubscriptionDetailsDto>
            {
                Items = dtoItems,
                TotalCount = totalCount,
                Page = request.Page.Page,
                PageSize = request.Page.PageSize,
            };

            return Result<PagedData<SubscriptionDetailsDto>>.Success(pagedData, _operation);
        }
    }
}
