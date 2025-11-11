using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAll;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetById
{
    public class GetSubDetailsByIdQueryHandler : IRequestHandler<GetSubDetailsByIdQuery, Result<SubscriptionDetailsDto>>
    {
        private readonly string _operation = OperationType.Get.ToString();
        private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;
        private readonly IMapper _mapper;
        private readonly SubDetailsManagementService _subscriptionDetailsMangeService;

        public GetSubDetailsByIdQueryHandler(
            ISubscriptionDetailsRepository subscriptionDetailsRepository,
            SubDetailsManagementService managementService,
            IMapper mapper
            )
        {
            _subscriptionDetailsRepository = subscriptionDetailsRepository;
            _subscriptionDetailsMangeService = managementService;
            _mapper = mapper;
        }

        public async Task<Result<SubscriptionDetailsDto>> Handle(GetSubDetailsByIdQuery request, CancellationToken cancellationToken)
        {
            var subDetails = await _subscriptionDetailsRepository.GetFullSubscriptionDetails(request.Id)
                ?? throw new SubscriptionDetailsNotFoundException(request.Id.ToString());

            cancellationToken.ThrowIfCancellationRequested();

            var subDetailsDto = _mapper.Map<SubscriptionDetailsDto>(subDetails)
                ?? throw new AutoMapperMappingException("Error occurred while mapping.");

            cancellationToken.ThrowIfCancellationRequested();

            return Result<SubscriptionDetailsDto>.Success(subDetailsDto, _operation);
        }
    }
}
