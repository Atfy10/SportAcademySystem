using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Queries.SportQueries.GetAvailableSportsForBranch
{
    public class GetAvailableSportsForBranchQueryHandler : IRequestHandler<GetAvailableSportsForBranchQuery, Result<List<SportDto>>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly IMapper _mapper;
        private readonly string _operation = OperationType.Get.ToString();

        public GetAvailableSportsForBranchQueryHandler(ISportRepository sportRepository,
            IMapper mapper)
        {
            _sportRepository = sportRepository;
            _mapper = mapper;
        }
        public async Task<Result<List<SportDto>>> Handle(GetAvailableSportsForBranchQuery request, CancellationToken cancellationToken)
        {
            if (request.branchId <= 0)
                throw new ArgumentException("Branch ID must be greater than zero.", nameof(request.branchId));
            
            var sports = await _sportRepository
                .GetAvailableSportsForBranch(request.branchId, cancellationToken) ?? [];

            var sportsDto = _mapper.Map<List<SportDto>>(sports) 
                ?? throw new AutoMapperMappingException();

            return Result<List<SportDto>>.Success(sportsDto, _operation);
        }
    }
}
