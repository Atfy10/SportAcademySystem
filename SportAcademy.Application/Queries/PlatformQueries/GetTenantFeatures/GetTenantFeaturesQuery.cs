using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenantFeatures;

public record GetTenantFeaturesQuery(Guid TenantId) : IRequest<Result<List<TenantFeatureResponse>>>;
