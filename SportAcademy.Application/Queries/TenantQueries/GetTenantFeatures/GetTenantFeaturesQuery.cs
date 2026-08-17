using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantFeatures;

public record GetTenantFeaturesQuery : IRequest<Result<TenantFeaturesListDto>>;
