using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.DTOs.TenantDtos;

public record ImportTenantSettingsCommand(ExportTenantSettingsDto Data) : IRequest<Result>;
