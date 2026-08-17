using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantSettingsOptions;

public record GetTenantSettingsOptionsQuery : IRequest<Result<TenantSettingsOptionsDto>>;
