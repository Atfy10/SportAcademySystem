using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.BranchQueries.SearchBranches;

public class SearchBranchesQueryHandler : IRequestHandler<SearchBranchesQuery, Result<PagedData<BranchCardDto>>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly string _operation = OperationType.GetAll.ToString();

    public SearchBranchesQueryHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<Result<PagedData<BranchCardDto>>> Handle(SearchBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _branchRepository.SearchAsync(request.Term, request.Page, cancellationToken);
        return Result<PagedData<BranchCardDto>>.Success(branches, _operation);
    }
}
