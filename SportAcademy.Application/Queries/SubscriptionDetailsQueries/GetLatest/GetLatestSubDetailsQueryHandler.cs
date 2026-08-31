using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings.Manual;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetLatest
{
    public class GetLatestSubDetailsQueryHandler : IRequestHandler<GetLatestSubDetailsQuery, Result<PagedData<SubscriptionDetailsDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentLanguageProvider _languageProvider;

        public GetLatestSubDetailsQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            IMapper mapper,
            ICurrentLanguageProvider languageProvider)
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _mapper = mapper;
            _languageProvider = languageProvider;
        }

        public async Task<Result<PagedData<SubscriptionDetailsDto>>> Handle(GetLatestSubDetailsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _subscriptionDetailsRepository.GetLatestSubscriptionsAsync(
                request.Page, request.Term, cancellationToken);

            // Manual mapper (not AutoMapper.Map) - see SubscriptionDetailsMapper for why: it
            // resolves Sport/Branch through their translation tables using the current request
            // language, which a declarative AutoMapper profile can't take as a parameter.
            var dtoItems = items.Select(sd => SubscriptionDetailsMapper.ToDto(sd, _languageProvider.Language)).ToList();

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
