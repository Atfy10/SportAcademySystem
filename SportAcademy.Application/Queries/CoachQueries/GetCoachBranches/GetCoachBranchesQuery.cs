using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;

namespace SportAcademy.Application.Queries.CoachQueries.GetCoachBranches;

public record GetCoachBranchesQuery(int CoachId) : IRequest<Result<List<CoachBranchAccessDto>>>;
