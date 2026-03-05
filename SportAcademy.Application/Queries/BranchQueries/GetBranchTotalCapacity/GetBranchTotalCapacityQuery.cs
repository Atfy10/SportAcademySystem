using MediatR;
using SportAcademy.Application.Common.Result;

public record GetBranchTotalCapacityQuery(int BranchId)
    : IRequest<Result<int>>;