using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;

namespace SportAcademy.Application.Queries.BranchQueries.GetBranchStats;

public record GetBranchStatsQuery(int BranchId) : IRequest<Result<BranchStatsDto>>;
