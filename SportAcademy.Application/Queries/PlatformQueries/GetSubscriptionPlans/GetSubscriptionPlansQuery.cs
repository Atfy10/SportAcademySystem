using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery : IRequest<Result<List<SubscriptionPlanSummaryDto>>>;
