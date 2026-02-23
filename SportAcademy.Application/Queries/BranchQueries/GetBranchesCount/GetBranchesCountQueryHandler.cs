using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Queries.BranchQueries.GetBranchesCount
{
    public class GetBranchesCountQueryHandler : IRequestHandler<GetBranchesCountQuery, Result<int>>
    {
        private readonly IBranchRepository _branchRepository;
        public GetBranchesCountQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }
        public async Task<Result<int>> Handle(GetBranchesCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _branchRepository.GetBranchesCountAsync(cancellationToken);
            return Result<int>.Success(count, nameof(GetBranchesCountQuery));
        }
    }
}
