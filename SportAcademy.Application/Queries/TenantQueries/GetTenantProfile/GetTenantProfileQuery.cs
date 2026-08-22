using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantProfile;

public record GetTenantProfileQuery : IRequest<Result<TenantProfileDto>>;
