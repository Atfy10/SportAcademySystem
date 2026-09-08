using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetPlanFeatures;

public record GetPlanFeaturesQuery(int PlanId) : IRequest<Result<List<PlanFeatureResponse>>>;
