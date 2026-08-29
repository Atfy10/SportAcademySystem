using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Application.Queries.SportQueries.GetAvailableSportsForBranch
{
    public class GetAvailableSportsForBranchQueryHandler : IRequestHandler<GetAvailableSportsForBranchQuery, Result<List<SportDto>>>
    {
        private readonly ISportRepository _sportRepository;
        private readonly IBranchRepository _brancRepository;
        private readonly string _operation = OperationType.Get.ToString();

        public GetAvailableSportsForBranchQueryHandler(
            ISportRepository sportRepository,
            IBranchRepository branchRepository)
        {
            _sportRepository = sportRepository;
            _brancRepository = branchRepository;
        }
        public async Task<Result<List<SportDto>>> Handle(GetAvailableSportsForBranchQuery request, CancellationToken cancellationToken)
        {
            if (request.branchId <= 0)
                throw new ArgumentException("Branch ID must be greater than zero.", nameof(request.branchId));

            var isBranchExist = await _brancRepository.IsExistAsync(request.branchId,
                cancellationToken);
            if (!isBranchExist)
                throw new BranchNotFoundException(request.branchId.ToString());

            var sportsDto = await _sportRepository
                .GetAvailableSportsForBranchTranslatedAsync(request.branchId, cancellationToken);

            return Result<List<SportDto>>.Success(sportsDto.ToList(), _operation);
        }
    }
}
