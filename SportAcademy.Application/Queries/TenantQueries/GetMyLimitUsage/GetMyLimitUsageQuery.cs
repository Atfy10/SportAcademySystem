using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetMyLimitUsage;

// The current tenant's plan limits and how much of each is used - read by the console so an
// "Add trainee" button can say "48/50" and warn at the cap instead of failing on save with a
// 403. TenantId comes from the caller's own JWT claim, never a parameter (same self-service rule
// as GetMyLimitReconciliationQuery).
public record GetMyLimitUsageQuery : IRequest<Result<List<TenantLimitResponse>>>;
