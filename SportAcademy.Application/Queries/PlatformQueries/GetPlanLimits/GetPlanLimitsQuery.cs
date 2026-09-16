using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetPlanLimits;

public record GetPlanLimitsQuery(int PlanId) : IRequest<Result<List<PlanLimitResponse>>>;
