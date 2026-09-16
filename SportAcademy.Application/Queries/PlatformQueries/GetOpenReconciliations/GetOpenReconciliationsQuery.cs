using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetOpenReconciliations;

// Platform-wide list, soonest-deadline-first - PLAN_LIMITS_DESIGN.md §5.3's "open-reconciliations
// list with deadlines, sorted soonest-first".
public record GetOpenReconciliationsQuery : IRequest<Result<List<LimitReconciliationResponse>>>;
