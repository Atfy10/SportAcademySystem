using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Application.Queries.BranchQueries.GetById
{
    public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly string _operationType = OperationType.Get.ToString();

        public GetBranchByIdQueryHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new BranchNotFoundException($"{request.Id}");

            var branchDto = branch.ToDto();
            return Result<BranchDto>.Success(branchDto, _operationType);
        }
    }
}
