using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.BranchExceptions;

public class GetBranchTotalCapacityQueryHandler
    : IRequestHandler<GetBranchTotalCapacityQuery, Result<int>>
{
    private readonly IBranchRepository _branchRepository;

    public GetBranchTotalCapacityQueryHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<Result<int>> Handle(
        GetBranchTotalCapacityQuery request,
        CancellationToken cancellationToken)
    {
        var isExist = await _branchRepository.IsExistAsync(request.BranchId, cancellationToken);
        if (!isExist)
            throw new BranchNotFoundException(request.BranchId.ToString());

        var totalCapacity = await _branchRepository
            .GetBranchTotalCapacityAsync(request.BranchId, cancellationToken);

        return Result<int>.Success(
            totalCapacity,
            nameof(GetBranchTotalCapacityQuery));
    }
}