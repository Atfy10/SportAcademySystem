using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenantLimits;

public record GetTenantLimitsQuery(Guid TenantId) : IRequest<Result<List<TenantLimitResponse>>>;
