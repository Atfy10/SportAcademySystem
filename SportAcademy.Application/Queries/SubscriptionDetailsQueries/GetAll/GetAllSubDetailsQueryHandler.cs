using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAll
{
    public class GetAllSubDetailsQueryHandler : IRequestHandler<GetAllSubDetailsQuery, Result<List<SubscriptionDetailsDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;

        public GetAllSubDetailsQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService,
            IMapper mapper
            )
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
            _mapper = mapper;
        }

        public async Task<Result<List<SubscriptionDetailsDto>>> Handle(GetAllSubDetailsQuery request, CancellationToken cancellationToken)
        {
            var subDetailsList = await _subscriptionDetailsRepository.GetAllFullSubDetailsAsync(cancellationToken)
                ?? [];

            cancellationToken.ThrowIfCancellationRequested();

            var subDetailsDtoList = _mapper.Map<List<SubscriptionDetailsDto>>(subDetailsList)
                ?? [];

            cancellationToken.ThrowIfCancellationRequested();

            return Result<List<SubscriptionDetailsDto>>.Success(subDetailsDtoList, _operation);
        }
    }
}
