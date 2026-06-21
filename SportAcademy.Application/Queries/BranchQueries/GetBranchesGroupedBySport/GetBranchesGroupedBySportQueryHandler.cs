using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.BranchQueries.GetBranchesGroupedBySport
{
    public class GetBranchesGroupedBySportQueryHandler : IRequestHandler<GetBranchesGroupedBySportQuery, Result<List<SportBranchGroupDto>>>
    {
        private readonly string _operation = OperationType.GetAll.ToString();
        private readonly ISportBranchRepository _sportBranchRepository;

        public GetBranchesGroupedBySportQueryHandler(ISportBranchRepository sportBranchRepository)
        {
            _sportBranchRepository = sportBranchRepository;
        }

        public async Task<Result<List<SportBranchGroupDto>>> Handle(GetBranchesGroupedBySportQuery request, CancellationToken cancellationToken)
        {
            var sportBranches = await _sportBranchRepository.GetAllWithBranchesAsync(cancellationToken);

            var dtos = sportBranches
                .Where(sb => sb.Branch != null)
                .Select(sb => new SportBranchGroupDto(sb.SportId, sb.BranchId, sb.Branch.Name))
                .ToList();

            return Result<List<SportBranchGroupDto>>.Success(dtos, _operation);
        }
    }
}
