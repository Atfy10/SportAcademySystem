using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;

namespace SportAcademy.Application.Queries.TenantQueries.ExportTenantSettings;

public record ExportTenantSettingsQuery : IRequest<Result<ExportTenantSettingsDto>>;
