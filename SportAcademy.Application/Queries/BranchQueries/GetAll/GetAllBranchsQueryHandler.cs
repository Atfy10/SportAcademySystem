using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.BranchQueries.GetAll
{
    public class GetAllBranchsQueryHandler : IRequestHandler<GetAllBranchesQuery, Result<List<BranchDropDownListDto>>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly string _operationType = OperationType.GetAll.ToString();

        public GetAllBranchsQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<Result<List<BranchDropDownListDto>>> Handle(GetAllBranchesQuery request, CancellationToken cancellationToken)
        {
            var branchesDto = await _branchRepository.GetAllBranchsBase(cancellationToken)??[];

            return Result<List<BranchDropDownListDto>>.Success(branchesDto, nameof(GetAllBranchesQuery));
        }
    }
}
