using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PublicDtos;

namespace SportAcademy.Application.Queries.PublicQueries.GetPublicPlans;

// Anonymous, public-marketing-site read of the plan catalog - deliberately its own query
// (not GetSubscriptionPlansQuery reused) because the two have different filters (publicly
// listed + active vs. just active) and different DTOs (this one includes granted feature
// names for the comparison table; the platform picker DTO doesn't need those).
public record GetPublicPlansQuery : IRequest<Result<List<PublicPlanDto>>>;
