using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantSettings;

public record GetTenantSettingsQuery : IRequest<Result<TenantSettingsDto>>;
