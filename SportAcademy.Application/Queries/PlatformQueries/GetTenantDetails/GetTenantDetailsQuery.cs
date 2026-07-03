using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;

namespace SportAcademy.Application.Queries.PlatformQueries.GetTenantDetails;

public record GetTenantDetailsQuery(Guid TenantId) : IRequest<Result<TenantDetailResponse>>;
